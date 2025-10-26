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
using TestSystem.Models;

namespace TestSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для TeacherDashboardPage.xaml
    /// </summary>
    public partial class TeacherDashboardPage : Page
    {
        private User _currentUser;

        public TeacherDashboardPage(User user)
        {
            InitializeComponent();
            _currentUser = user;
            WelcomeText.Text = $"Вітаємо, {user.FullName}!";
        }
    }
}
