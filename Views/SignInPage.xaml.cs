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
using TestSystem.Views;
using TestSystem.Views.Pages;

namespace TestSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для SignInPage.xaml
    /// </summary>
    public partial class SignInPage : Page
    {
        public User? CurrentUser { get; private set; }

        public SignInPage()
        {
            InitializeComponent();
            Loaded += (s, e) => LoginBox.Focus();
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = LoginBox.Text.Trim();
                string pass = PasswordBox.Password;

                if (string.IsNullOrWhiteSpace(login))
                {
                    ShowStatus("Введіть логін.", false);
                    LoginBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(pass))
                {
                    ShowStatus("Введіть пароль.", false);
                    PasswordBox.Focus();
                    return;
                }

                var user = AuthStore.TrySignIn(login, pass);
                if (user != null)
                {
                    CurrentUser = user;
                    NavigateAfterLogin(user);
                }
                else
                {
                    ShowStatus("Невірний логін або пароль.", false);
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Помилка входу: {ex.Message}", false);
            }
        }

        private void NavigateAfterLogin(User user)
        {
            var mainWindow = (MainWindow)Window.GetWindow(this);

            if (user.UserType == "Teacher")
            {
                mainWindow.NavigateToPage(new TeacherDashboardPage(user));
            }
            else
            {
                mainWindow.NavigateToPage(new StudentDashboardPage(user));
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void GoToSignUp_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.GetWindow(this);
            mainWindow.NavigateToPage(new SignUpPage());
        }

        private void ShowStatus(string message, bool isSuccess = true)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = isSuccess ?
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green) :
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
        }

        // Метод для заповнення даних після реєстрації
        public void FillCredentials(string login, string password)
        {
            LoginBox.Text = login;
            PasswordBox.Password = password;
            ShowStatus("Дані заповнено автоматично. Натисніть 'Увійти'.", true);
            PasswordBox.Focus();
        }
    }
}

