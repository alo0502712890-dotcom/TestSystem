using System.Windows;
using System.Windows.Controls;
using TestSystem.Data;
using TestSystem.Models;

namespace TestSystem.Views.Pages
{

    public partial class StudentDashboardPage : Page
    {
        private User _currentUser;
        private TestRepository _repository;

        // Приймаємо автентифікованого користувача
        public StudentDashboardPage(User user)
        {
            InitializeComponent();
            _currentUser = user;

            // Ініціалізація контексту та репозиторію
            _repository = new TestRepository(new TestSystemContext());

            // Відображення
            WelcomeText.Text = $"Вітаємо, {user.FullName}!";
            UserNameText.Text = $"Логін: {user.Login}";

            this.Loaded += (s, e) => LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            // 1. Доступні тести
            var availableTests = _repository.GetAvailableTests(_currentUser.UserId);
            TestsListView.ItemsSource = availableTests;
            NoTestsText.Visibility = availableTests.Any() ? Visibility.Collapsed : Visibility.Visible;

            // 2. Активні спроби
            var activeSessions = _repository.GetActiveSessions(_currentUser.UserId);
            ActiveSessionsListView.ItemsSource = activeSessions;
            NoActiveSessionsText.Visibility = activeSessions.Any() ? Visibility.Collapsed : Visibility.Visible;

            // 3. Історія тестувань
            var history = _repository.GetTestHistory(_currentUser.UserId);
            HistoryDataGrid.ItemsSource = history;
            NoHistoryText.Visibility = history.Any() ? Visibility.Collapsed : Visibility.Visible;

            StatusText.Text = "Дані завантажено успішно.";
        }

        private void StartTest_Click(object sender, RoutedEventArgs e)
        {
            Button startButton = sender as Button;
            Test selectedTest = startButton?.DataContext as Test;

            if (selectedTest != null)
            {
                int newSessionID = _repository.StartNewTestSession(_currentUser.UserId, selectedTest.TestID);

                if (this.NavigationService != null)
                {
                    // 1. Створюємо екземпляр TestPage, передаючи ID нової сесії.
                    var testPage = new TestPage(newSessionID);

                    // 2. Виконуємо навігацію.
                    this.NavigationService.Navigate(testPage);
                }

                LoadDashboardData(); // Оновлюємо списки, щоб нова сесія відобразилася в "Активні спроби"
            }
        }

        private void ContinueTest_Click(object sender, RoutedEventArgs e)
        {
            Button continueButton = sender as Button;
            TestSessionInfo activeSession = continueButton?.DataContext as TestSessionInfo;

            if (activeSession != null)
            {
                if (activeSession != null)
                {
                    if (this.NavigationService != null)
                    {
                        // 1. Створюємо екземпляр TestPage, передаючи ID існуючої сесії.
                        var testPage = new TestPage(activeSession.SessionID);

                        // 2. Виконуємо навігацію.
                        this.NavigationService.Navigate(testPage);
                    }
                }
            }
        }


        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null)
            {
                try
                {
                    var signInPage = new SignInPage();

                    this.NavigationService.Navigate(signInPage);

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при переході на сторінку входу: {ex.Message}", "Помилка Навігації");
                }
            }
        }
    }
}