using System.Windows.Controls;
using TestSystem.Models;

namespace TestSystem.Views.Pages
{
    public partial class StudentDashboardPage : Page
    {
        private User _currentUser;

        public StudentDashboardPage(User user)
        {
            InitializeComponent();
            _currentUser = user;

            // Просте заповнення даних
            WelcomeText.Text = $"Вітаємо, {user.FullName}!";
            UserNameText.Text = $"Логін: {user.Login}";
            StatusText.Text = "Готовий до тестування";
        }
    }
}