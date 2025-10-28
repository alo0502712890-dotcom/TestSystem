using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestSystem.Data;
using TestSystem.Models;

namespace TestSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для TeacherDashboardPage.xaml
    /// </summary>
    public partial class TeacherDashboardPage : Page
    {
        private User _currentUser;
        private List<Test> _tests;

        public TeacherDashboardPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            Loaded += TeacherTestsPage_Loaded;
        }

        private void TeacherTestsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTests();
        }


        // Завантажує список тестів викладача з бази даних
        private void LoadTests()
        {
            try
            {
                _tests = TestService.GetTeacherTests(_currentUser.UserId);
                TestsListView.ItemsSource = _tests;
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження тестів: {ex.Message}", "Помилка");
            }
        }

        // Оновлює елементи інтерфейсу(кількість тестів, видимість панелі "немає тестів")
        private void UpdateUI()
        {
            int testsCount = _tests?.Count ?? 0;
            TestsCountText.Text = $"Знайдено тестів: {testsCount}";

            // Показуємо/ховаємо панель "немає тестів"
            if (testsCount == 0)
            {
                NoTestsPanel.Visibility = Visibility.Visible;
                TestsListView.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoTestsPanel.Visibility = Visibility.Collapsed;
                TestsListView.Visibility = Visibility.Visible;
            }
        }


        // Навігація на сторінку створення нового тесту
        private void CreateNewTest_Click(object sender, RoutedEventArgs e)
        {
            var createTestPage = new CreateEditTestPage(_currentUser);
            var mainWindow = (MainWindow)Window.GetWindow(this);
            mainWindow.NavigateToPage(createTestPage);
        }


        // Навігація на сторінку редагування обраного тесту
        private void EditTest_Click(object sender, RoutedEventArgs e)
        {
            if (TestsListView.SelectedItem is Test selectedTest)
            {
                try
                {
                    // 1. Завантажуємо повний об'єкт тесту, включаючи питання та відповіді
                    Test testToEdit = TestService.GetTestWithDetails(selectedTest.TestID);

                    if (testToEdit == null)
                    {
                        MessageBox.Show("Помилка: Не вдалося знайти деталі тесту.", "Помилка");
                        return;
                    }

                    // 2. Навігація на сторінку редагування з повним об'єктом
                    var editTestPage = new CreateEditTestPage(_currentUser, testToEdit);
                    var mainWindow = (MainWindow)Window.GetWindow(this);
                    mainWindow.NavigateToPage(editTestPage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка завантаження деталей тесту: {ex.Message}", "Помилка");
                }
            }
            else
            {
                MessageBox.Show("Оберіть тест для редагування", "Інформація");
            }
        }

        // Обробник для перегляду статистики обраного тесту
        private void ViewStatistics_Click(object sender, RoutedEventArgs e)
        {
            if (TestsListView.SelectedItem is Test selectedTest)
            {
                // сторінка статистики
                MessageBox.Show($"Статистика для тесту '{selectedTest.TestName}':\n" +
                              "• Кількість спроб: 0\n" +
                              "• Середній бал: 0\n" +
                              "• Успішність: 0%",
                              $"Статистика: {selectedTest.TestName}");
            }
        }


        // Обробник для видалення обраного тесту
        private void DeleteTest_Click(object sender, RoutedEventArgs e)
        {
            if (TestsListView.SelectedItem is Test selectedTest)
            {
                var result = MessageBox.Show($"Ви дійсно бажаєте видалити тест '{selectedTest.TestName}'?\n\n" +
                                           "Ця дія незворотня!",
                                           "Підтвердження видалення",
                                           MessageBoxButton.YesNo,
                                           MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (TestService.DeleteTest(selectedTest.TestID))
                        {
                            MessageBox.Show("Тест успішно видалено", "Успіх");
                            LoadTests();
                            StatusText.Text = $"Тест '{selectedTest.TestName}' видалено";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Помилка видалення тесту: {ex.Message}", "Помилка");
                    }
                }
            }
        }


        // Обробник зміни обраного елемента у списку тестів
        private void TestsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TestsListView.SelectedItem is Test selectedTest)
            {
                StatusText.Text = $"Обрано тест: {selectedTest.TestName}. " +
                                $"Питань: {selectedTest.Questions?.Count ?? 0}. " +
                                $"Статус: {(selectedTest.IsActive ? "Активний" : "Неактивний")}";
            }
            else
            {
                StatusText.Text = "Оберіть тест для перегляду дій";
            }
        }


        // Обробник кнопки "Вихід"
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.GetWindow(this);

            if (mainWindow != null)
            {
                try
                {
                    mainWindow.NavigateToPage(new SignInPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при переході на сторінку входу: {ex.Message}", "Помилка Навігації");
                }
            }
        }
    }
}
