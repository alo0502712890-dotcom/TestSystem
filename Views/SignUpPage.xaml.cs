using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using TestSystem.Data;
using TestSystem.Models;
using TestSystem.Views;

namespace TestSystem.Views.Pages
{
    public partial class SignUpPage : Page
    {
        public string RegisteredLogin { get; private set; } = string.Empty;
        public string RegisteredPassword { get; private set; } = string.Empty;

        public SignUpPage()
        {
            InitializeComponent();
            Loaded += (s, e) => EmailBox.Focus();
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailBox.Text.Trim();
            var login = LoginBox.Text.Trim();
            var fullName = FullNameBox.Text.Trim();
            var pass = PasswordBox.Password; // Використовуємо Password для PasswordBox
            var confirm = ConfirmBox.Password; // Використовуємо Password для PasswordBox

            // Перевірки
            if (!ValidateInput(email, login, fullName, pass, confirm))
                return;

            string userType = IsTeacherRadio.IsChecked == true ? "Teacher" : "Student";

            bool created = AuthStore.RegisterUser(login, pass, fullName, email, userType);

            if (created)
            {
                RegisteredLogin = login;
                RegisteredPassword = pass;

                ShowStatus("Акаунт успішно створено!", true);

                // Повертаємось на сторінку входу з заповненими даними
                ReturnToLoginWithCredentials();
            }
            else
            {
                ShowStatus("Такий логін або email вже існує!", false);
                LoginBox.Focus();
            }
        }

        private bool ValidateInput(string email, string login, string fullName, string pass, string confirm)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                ShowStatus("Введіть логін.", false);
                LoginBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowStatus("Введіть повне ім'я.", false);
                FullNameBox.Focus();
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowStatus("Некоректний email.", false);
                EmailBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(pass) || pass.Length < 4)
            {
                ShowStatus("Пароль має містити щонайменше 4 символи.", false);
                PasswordBox.Focus();
                return false;
            }

            if (pass != confirm)
            {
                ShowStatus("Паролі не співпадають.", false);
                ConfirmBox.Focus();
                return false;
            }

            return true;
        }

        private void ReturnToLoginWithCredentials()
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                var loginPage = new SignInPage();
                // Заповнюємо дані на сторінці входу
                loginPage.FillCredentials(RegisteredLogin, RegisteredPassword);
                mainWindow.NavigateToPage(loginPage);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.NavigateToPage(new SignInPage());
            }
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            if (StatusTextBlock != null)
            {
                StatusTextBlock.Text = message;
                StatusTextBlock.Foreground = isSuccess ?
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green) :
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}