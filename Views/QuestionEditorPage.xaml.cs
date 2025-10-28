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
using Microsoft.Win32;
using TestSystem.Models;

namespace TestSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для QuestionEditorPage.xaml
    /// </summary>
    public partial class QuestionEditorPage : Page
    {
        private List<Answer> _answers;
        private Question _currentQuestion;
        public Question CreatedQuestion { get; private set; }

        public QuestionEditorPage() : this(null) { }

        public QuestionEditorPage(Question question)
        {
            InitializeComponent();
            _currentQuestion = question;

            if (_currentQuestion == null)
            {
                _answers = new List<Answer>();
                WeightTextBox.Text = "1"; // значення за замовчуванням
            }
            else
            {
                // Редагуємо існуюче питання
                _answers = new List<Answer>(_currentQuestion.Answers ?? new List<Answer>());
                LoadQuestionData();
            }

            AnswersListView.ItemsSource = _answers;
        }


        private void LoadQuestionData()
        {
            if (_currentQuestion == null)
                return; // для нового питання нічого не завантажуємо

            QuestionTextTextBox.Text = _currentQuestion.QuestionText;
            WeightTextBox.Text = _currentQuestion.Weight.ToString();
            ImagePathTextBox.Text = _currentQuestion.ImagePath ?? "";

            foreach (ComboBoxItem item in QuestionTypeComboBox.Items)
            {
                if (item.Tag.ToString() == _currentQuestion.QuestionType)
                {
                    QuestionTypeComboBox.SelectedItem = item;
                    break;
                }
            }

            RefreshAnswersList();
        }

        private void AddAnswer_Click(object sender, RoutedEventArgs e)
        {
            // Відкриваємо вікно для введення тексту нової відповіді
            var newText = Microsoft.VisualBasic.Interaction.InputBox(
                "Введіть текст нової відповіді:",
                "Нова відповідь",
                "");

            if (!string.IsNullOrWhiteSpace(newText))
            {
                _answers.Add(new Answer
                {
                    AnswerText = newText,
                    IsCorrect = false,
                    SortOrder = _answers.Count + 1
                });

                RefreshAnswersList();
            }
        }

        private void DeleteAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (AnswersListView.SelectedItem is Answer selectedAnswer)
            {
                _answers.Remove(selectedAnswer);
                RefreshAnswersList();
            }
        }

        private void RefreshAnswersList()
        {
            AnswersListView.ItemsSource = null;
            AnswersListView.ItemsSource = _answers;
        }

        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Зображення (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Оберіть зображення для питання"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImagePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            // Якщо ми редагуємо існуюче питання
            if (_currentQuestion != null)
            {
                _currentQuestion.QuestionText = QuestionTextTextBox.Text.Trim();
                _currentQuestion.QuestionType = ((ComboBoxItem)QuestionTypeComboBox.SelectedItem).Tag.ToString();
                _currentQuestion.Weight = int.Parse(WeightTextBox.Text);
                _currentQuestion.ImagePath = ImagePathTextBox.Text;
                _currentQuestion.Answers = new List<Answer>(_answers);
                CreatedQuestion = _currentQuestion; // повертаємо той самий об’єкт!
            }
            else
            {
                // Створюємо нове питання
                CreatedQuestion = new Question
                {
                    QuestionText = QuestionTextTextBox.Text.Trim(),
                    QuestionType = ((ComboBoxItem)QuestionTypeComboBox.SelectedItem).Tag.ToString(),
                    Weight = int.Parse(WeightTextBox.Text),
                    ImagePath = ImagePathTextBox.Text,
                    Answers = new List<Answer>(_answers)
                };
            }

            var mainWindow = (MainWindow)Window.GetWindow(this);
            mainWindow.GoBack();
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Window.GetWindow(this);
            mainWindow.GoBack();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(QuestionTextTextBox.Text))
            {
                MessageBox.Show("Введіть текст питання", "Помилка");
                QuestionTextTextBox.Focus();
                return false;
            }

            if (_answers.Count == 0)
            {
                MessageBox.Show("Додайте хоча б один варіант відповіді", "Помилка");
                return false;
            }

            if (!int.TryParse(WeightTextBox.Text, out int weight) || weight <= 0)
            {
                MessageBox.Show("Введіть коректну вагу питання", "Помилка");
                WeightTextBox.Focus();
                return false;
            }

            if (((ComboBoxItem)QuestionTypeComboBox.SelectedItem).Tag.ToString() == "Single")
            {
                int correctAnswers = _answers.Count(a => a.IsCorrect);
                if (correctAnswers != 1)
                {
                    MessageBox.Show("Для типу 'Один варіант відповіді' має бути рівно одна правильна відповідь", "Помилка");
                    return false;
                }
            }

            return true;
        }

        private void AnswersListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AnswersListView.SelectedItem is Answer selectedAnswer)
            {
                var newText = Microsoft.VisualBasic.Interaction.InputBox(
                    "Введіть текст відповіді:",
                    "Редагування відповіді",
                    selectedAnswer.AnswerText);

                if (!string.IsNullOrWhiteSpace(newText))
                {
                    selectedAnswer.AnswerText = newText;
                    RefreshAnswersList();
                }
            }
        }

        private void IsCorrect_Checked(object sender, RoutedEventArgs e)
        {
            if (((ComboBoxItem)QuestionTypeComboBox.SelectedItem)?.Tag?.ToString() == "Single")
            {
                // Якщо обрано тип "Один варіант відповіді"
                var currentCheckbox = (CheckBox)sender;
                var currentAnswer = (Answer)currentCheckbox.DataContext;

                foreach (var answer in _answers)
                {
                    if (answer != currentAnswer)
                    {
                        answer.IsCorrect = false;
                    }
                }

                RefreshAnswersList();
            }
        }

    }
}
