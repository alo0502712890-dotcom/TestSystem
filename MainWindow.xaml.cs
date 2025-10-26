using System.Text;
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

namespace TestSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Завантажуємо стартову сторінку
            MainFrame.Navigate(new SignInPage());
        }

        // Метод для навігації між сторінками
        public void NavigateToPage(Page page)
        {
            MainFrame.Navigate(page);
        }

        // Метод для повернення назад
        public void GoBack()
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }
    }
}