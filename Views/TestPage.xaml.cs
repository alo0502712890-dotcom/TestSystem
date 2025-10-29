using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TestSystem.Data;
using TestSystem.Models;

namespace TestSystem.Views
{
    /// <summary>
    /// Логика взаимодействия для TestPage.xaml
    /// </summary>

    public partial class TestPage : Page
    {
        private readonly int _sessionID;
        private readonly TestRepository _repository;
        private List<Question> _questions = new List<Question>();
        private int _currentQuestionIndex = 0;
        private Dictionary<int, List<int>> _userSelections = new Dictionary<int, List<int>>();

        private DispatcherTimer? _timer;
        private TimeSpan _timeLeft;

        public TestPage(int sessionID)
        {
            InitializeComponent();
            _sessionID = sessionID;

            // Ініціалізація репозиторію
            _repository = new TestRepository(new TestSystemContext());

            LoadTestContent();
        }

        // =========================================================================
        // 1. МЕТОД ЗАВАНТАЖЕННЯ (LoadTestContent)
        // =========================================================================

        private void LoadTestContent()
        {
            _questions = _repository.GetTestQuestionsBySession(_sessionID);

            if (!_questions.Any())
            {
                MessageBox.Show("Помилка завантаження питань або тест порожній.", "Помилка");
                this.NavigationService?.GoBack();
                return;
            }

            var session = _repository.GetSessionWithTest(_sessionID);

            if (session?.Test != null)
            {
                TestNameBlock.Text = session.Test.TestName;
                if (session.Test.TimeLimit.HasValue && session.Test.TimeLimit.Value > 0)
                {
                    _timeLeft = TimeSpan.FromMinutes(session.Test.TimeLimit.Value);
                    StartTimer(); // ВИПРАВЛЕНО: Виклик тепер коректний
                }
                else
                {
                    TimerBlock.Text = "Без обмежень";
                }
            }

            DisplayCurrentQuestion(); // ВИПРАВЛЕНО: Виклик тепер коректний
        }

        // =========================================================================
        // 2. МЕТОДИ ТАЙМЕРА (StartTimer, Timer_Tick)
        // ВИПРАВЛЕНО: Винесені на рівень класу
        // =========================================================================

        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_timeLeft.TotalSeconds > 0)
            {
                _timeLeft = _timeLeft.Subtract(TimeSpan.FromSeconds(1));
                TimerBlock.Text = _timeLeft.ToString(@"hh\:mm\:ss");
            }
            else
            {
                _timer?.Stop();
                MessageBox.Show("Час тестування вийшов! Тест буде автоматично завершено.", "Час вийшов", MessageBoxButton.OK, MessageBoxImage.Warning);
                SubmitTest();
            }
        }

        // =========================================================================
        // 3. МЕТОДИ ВІДОБРАЖЕННЯ (DisplayCurrentQuestion, RenderAnswers)
        // ВИПРАВЛЕНО: Винесені на рівень класу
        // =========================================================================

        private void DisplayCurrentQuestion()
        {
            if (!_questions.Any() || _currentQuestionIndex < 0 || _currentQuestionIndex >= _questions.Count)
                return;

            Question currentQuestion = _questions[_currentQuestionIndex];

            // Показ картинки (якщо є)
            if (!string.IsNullOrEmpty(currentQuestion.ImagePath) && System.IO.File.Exists(currentQuestion.ImagePath))
            {
                QuestionImage.Source = new BitmapImage(new Uri(currentQuestion.ImagePath, UriKind.RelativeOrAbsolute));
                QuestionImage.Visibility = Visibility.Visible;
            }
            else
            {
                QuestionImage.Visibility = Visibility.Collapsed;
            }

            // Оновлення текстових блоків
            QuestionNumberBlock.Text = $"Питання {_currentQuestionIndex + 1} з {_questions.Count}. (Вага: {currentQuestion.Weight})";
            QuestionTextBlock.Text = currentQuestion.QuestionText;
            StatusText.Text = $"Питання {_currentQuestionIndex + 1} з {_questions.Count}";

            // Навігація
            PrevButton.IsEnabled = _currentQuestionIndex > 0;
            NextButton.Visibility = _currentQuestionIndex < _questions.Count - 1 ? Visibility.Visible : Visibility.Collapsed;
            FinishButton.Visibility = _currentQuestionIndex == _questions.Count - 1 ? Visibility.Visible : Visibility.Collapsed;

            RenderAnswers(currentQuestion);
        }

        private void RenderAnswers(Question question)
        {
            AnswersPanel.Children.Clear();
            bool isMultiple = question.QuestionType == "Multiple";

            _userSelections.TryGetValue(question.QuestionID, out List<int>? selectedAnswers);
            selectedAnswers ??= new List<int>();

            foreach (var answer in question.Answers.OrderBy(a => a.SortOrder))
            {
                ToggleButton control;
                if (isMultiple)
                {
                    control = new CheckBox();
                    control.Content = answer.AnswerText;
                    control.IsChecked = selectedAnswers.Contains(answer.AnswerID);
                    ((CheckBox)control).Checked += Answer_Checked; // ВИПРАВЛЕНО: Виклик коректний
                    ((CheckBox)control).Unchecked += Answer_Unchecked; // ВИПРАВЛЕНО: Виклик коректний
                }
                else
                {
                    // ВИПРАВЛЕННЯ GROUPNAME: Явне використання RadioButton
                    RadioButton radioButton = new RadioButton();
                    radioButton.GroupName = $"Q_{question.QuestionID}";
                    radioButton.Content = answer.AnswerText;
                    radioButton.IsChecked = selectedAnswers.Contains(answer.AnswerID);
                    radioButton.Checked += Answer_Checked; // ВИПРАВЛЕНО: Виклик коректний
                    control = radioButton;
                }

                control.Tag = new { AnswerID = answer.AnswerID, QuestionID = question.QuestionID };
                control.Margin = new Thickness(0, 5, 0, 5);
                AnswersPanel.Children.Add(control);
            }
        }

        // =========================================================================
        // 4. МЕТОДИ ЗБОРУ ВІДПОВІДЕЙ (Answer_Checked, Answer_Unchecked)
        // ВИПРАВЛЕНО: Винесені на рівень класу
        // =========================================================================

        private void Answer_Checked(object sender, RoutedEventArgs e)
        {
            var control = (ToggleButton)sender;
            var tagData = (dynamic)control.Tag;
            int qID = tagData.QuestionID;
            int aID = tagData.AnswerID;

            if (!_userSelections.ContainsKey(qID))
            {
                _userSelections[qID] = new List<int>();
            }

            if (control is RadioButton)
            {
                _userSelections[qID].Clear();
                _userSelections[qID].Add(aID);
            }
            else
            {
                if (!_userSelections[qID].Contains(aID))
                {
                    _userSelections[qID].Add(aID);
                }
            }
        }

        private void Answer_Unchecked(object sender, RoutedEventArgs e)
        {
            var control = (CheckBox)sender;
            var tagData = (dynamic)control.Tag;
            int qID = tagData.QuestionID;
            int aID = tagData.AnswerID;

            if (_userSelections.ContainsKey(qID))
            {
                _userSelections[qID].Remove(aID);
            }
        }

        // =========================================================================
        // 5. МЕТОДИ НАВІГАЦІЇ ТА ЗАВЕРШЕННЯ
        // =========================================================================

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex > 0)
            {
                _currentQuestionIndex--;
                DisplayCurrentQuestion();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex < _questions.Count - 1)
            {
                _currentQuestionIndex++;
                DisplayCurrentQuestion();
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Ви впевнені, що хочете завершити тестування?", "Завершення тесту", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {

                SubmitTest();
            }
        }

        private void SubmitTest()
        {
            _timer?.Stop();

            var allUserAnswers = new List<UserAnswer>();
            DateTime now = DateTime.Now;

            foreach (var kvp in _userSelections)
            {
                int qID = kvp.Key;
                foreach (int aID in kvp.Value)
                {
                    allUserAnswers.Add(new UserAnswer
                    {
                        SessionID = _sessionID,
                        QuestionID = qID,
                        AnswerID = aID,
                        AnsweredAt = now
                    });
                }
            }

            _repository.CompleteTestSession(_sessionID, allUserAnswers);

            if (this.NavigationService?.CanGoBack == true)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}
