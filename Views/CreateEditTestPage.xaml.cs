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
using Microsoft.EntityFrameworkCore;
using TestSystem.Data;
using TestSystem.Models;

namespace TestSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для CreateEditTestPage.xaml
    /// </summary>
    public partial class CreateEditTestPage : Page
    {
        private User _currentUser;
        private Test _currentTest;
        private List<Question> _questions;

        // Конструктор для створення нового тесту
        public CreateEditTestPage(User user) : this(user, null) { }

        // Конструктор для редагування існуючого тесту
        public CreateEditTestPage(User user, Test test)
        {
            InitializeComponent();
            _currentUser = user;
            _currentTest = test;
            _questions = new List<Question>();

            if (test != null)
            {
                LoadTestData();
                LoadQuestions();
            }
        }

        private void LoadTestData()
        {
            TestNameTextBox.Text = _currentTest.TestName;
            DescriptionTextBox.Text = _currentTest.Description;
            TimeLimitTextBox.Text = _currentTest.TimeLimit?.ToString() ?? "30";
            MaxAttemptsTextBox.Text = _currentTest.MaxAttempts.ToString();
            IsActiveCheckBox.IsChecked = _currentTest.IsActive;
        }

        private void LoadQuestions()
        {
            if (_currentTest.Questions != null)
            {
                _questions = _currentTest.Questions.ToList();
            }
            else
            {
                _questions = new List<Question>();
            }

            RefreshQuestionsList();
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            // Створюємо сторінку для додавання нового питання
            var editorPage = new QuestionEditorPage();

            // Підписуємося на подію "Повернення" після збереження
            editorPage.Loaded += (s, ev) =>
            {
                editorPage.Unloaded += (ss, ee) =>
                {
                    if (editorPage.CreatedQuestion != null)
                    {
                        // Додаємо нове питання лише після натискання "Зберегти"
                        editorPage.CreatedQuestion.SortOrder = _questions.Count + 1;
                        _questions.Add(editorPage.CreatedQuestion);
                        RefreshQuestionsList();
                    }
                };
            };

            NavigationService.Navigate(editorPage);
        }



        private void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsListView.SelectedItem is Question selectedQuestion)
            {
                // Навігація на сторінку редагування питання
                var editQuestionPage = new QuestionEditorPage(selectedQuestion);
                var mainWindow = (MainWindow)Window.GetWindow(this);
                mainWindow.NavigateToPage(editQuestionPage);
            }
            else
            {
                MessageBox.Show("Оберіть питання для редагування", "Інформація");
            }
        }

        private void DeleteQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionsListView.SelectedItem is Question selectedQuestion)
            {
                var result = MessageBox.Show("Видалити це питання?", "Підтвердження",
                                           MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _questions.Remove(selectedQuestion);
                    RefreshQuestionsList();
                }
            }
            else
            {
                MessageBox.Show("Оберіть питання для видалення", "Інформація");
            }
        }

        private void RefreshQuestionsList()
        {
            QuestionsListView.ItemsSource = null;
            QuestionsListView.ItemsSource = _questions;
        }

        private void SaveTest_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                if (_currentTest == null)
                {
                    // Створення нового тесту
                    _currentTest = new Test
                    {
                        TestName = TestNameTextBox.Text.Trim(),
                        Description = DescriptionTextBox.Text.Trim(),
                        CreatedBy = _currentUser.UserId,
                        CreatedDate = DateTime.Now,
                        TimeLimit = int.Parse(TimeLimitTextBox.Text),
                        MaxAttempts = int.Parse(MaxAttemptsTextBox.Text),
                        IsActive = IsActiveCheckBox.IsChecked == true,
                        Questions = _questions
                    };

                    int testId = TestService.CreateTest(_currentTest);

                    MessageBox.Show("Тест успішно створено!", "Успіх");
                }
                else
                {
                    // Оновлення існуючого тесту
                    _currentTest.TestName = TestNameTextBox.Text.Trim();
                    _currentTest.Description = DescriptionTextBox.Text.Trim();
                    _currentTest.TimeLimit = int.Parse(TimeLimitTextBox.Text);
                    _currentTest.MaxAttempts = int.Parse(MaxAttemptsTextBox.Text);
                    _currentTest.IsActive = IsActiveCheckBox.IsChecked == true;

                    TestService.UpdateTestDetails(_currentTest, _questions);

                    MessageBox.Show("Тест успішно оновлено!", "Успіх");
                }

                // Повертаємось до списку тестів
                var mainWindow = (MainWindow)Window.GetWindow(this);
                mainWindow.NavigateToPage(new TeacherDashboardPage(_currentUser));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження тесту: {ex.Message}", "Помилка");
            }
        }



        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.GetWindow(this);
            mainWindow.NavigateToPage(new TeacherDashboardPage(_currentUser));
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(TestNameTextBox.Text))
            {
                MessageBox.Show("Введіть назву тесту", "Помилка");
                TestNameTextBox.Focus();
                return false;
            }

            if (!int.TryParse(TimeLimitTextBox.Text, out int timeLimit) || timeLimit <= 0)
            {
                MessageBox.Show("Введіть коректний час на тест", "Помилка");
                TimeLimitTextBox.Focus();
                return false;
            }

            if (!int.TryParse(MaxAttemptsTextBox.Text, out int maxAttempts) || maxAttempts <= 0)
            {
                MessageBox.Show("Введіть коректну кількість спроб", "Помилка");
                MaxAttemptsTextBox.Text = "1";
                return false;
            }

            if (_questions.Count == 0)
            {
                MessageBox.Show("Додайте хоча б одне питання до тесту", "Помилка");
                return false;
            }

            return true;
        }

    }
}
