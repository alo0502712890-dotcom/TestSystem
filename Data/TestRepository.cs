using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using TestSystem.Models;

namespace TestSystem.Data
{
    public class TestRepository
    {
        private readonly TestSystemContext _context;

        public TestRepository(TestSystemContext context)
        {
            _context = context;
        }

        // --- Допоміжні методи для TestPage (доступ до сесії) ---
        public TestSession? GetSessionWithTest(int sessionID)
        {
            return _context.TestSessions
                .Include(s => s.Test)
                .FirstOrDefault(s => s.SessionID == sessionID);
        }

        // =========================================================================
        // МЕТОДИ ДЛЯ STUDENT DASHBOARD
        // =========================================================================

        public List<Test> GetAvailableTests(int userID)
        {
            // 1. Отримуємо всі активні тести
            var activeTests = _context.Tests
                .Where(t => t.IsActive)
                .ToList();

            // 2. Отримуємо історію сесій для цього користувача, групуючи їх за TestID
            var completedAttempts = _context.TestSessions
                .Where(s => s.UserID == userID) // Фільтр за поточним користувачем
                .GroupBy(s => s.TestID)
                .Select(g => new
                {
                    TestID = g.Key,
                    AttemptsCount = g.Count(s => s.IsCompleted) // Рахуємо тільки завершені спроби
                })
                .ToDictionary(x => x.TestID, x => x.AttemptsCount);

            // 3. Фільтруємо тести
            var availableTests = activeTests.Where(t =>
            {
                // Отримуємо кількість спроб, використаних для цього тесту
                int usedAttempts = completedAttempts.GetValueOrDefault(t.TestID);

                // Повертаємо тест, якщо використаних спроб менше, ніж дозволено (MaxAttempts)
                // Використовуємо .Value, оскільки MaxAttempts у вашій моделі Test є int,
                // але я припускаю, що його значення завжди > 0.
                return usedAttempts < t.MaxAttempts;
            });

            return availableTests
                .OrderBy(t => t.TestName)
                .ToList();
        }

        public List<TestSessionInfo> GetActiveSessions(int userID)
        {
            return _context.TestSessions
                    // === ВИПРАВЛЕННЯ: ЗАЛИШАЄМО ЛИШЕ НАЙПРОСТІШУ ПЕРЕВІРКУ ===
                    .Where(s => s.UserID == userID && !s.IsCompleted)

                    .Include(s => s.Test)
                    .Select(s => new TestSessionInfo
                    {
                        SessionID = s.SessionID,
                        TestName = s.Test != null ? s.Test.TestName : "Невідомий тест",
                        StartTime = s.StartTime,
                        IsCompleted = s.IsCompleted
                    })
                    .OrderByDescending(s => s.StartTime)
                    .ToList();
        }

        public List<TestSessionInfo> GetTestHistory(int userID)
        {
            return _context.TestSessions
                .Where(s => s.UserID == userID && s.IsCompleted && s.EndTime.HasValue)
                .Include(s => s.Test)
                .Select(s => new TestSessionInfo
                {
                    SessionID = s.SessionID,
                    TestName = s.Test != null ? s.Test.TestName : "Невідомий тест",
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Score = s.Score,
                    MaxScore = s.MaxScore,
                    IsCompleted = s.IsCompleted
                })
                .OrderByDescending(s => s.EndTime)
                .ToList();
        }

        public int StartNewTestSession(int userID, int testID)
        {
            var newSession = new TestSession
            {
                UserID = userID,
                TestID = testID,
                StartTime = DateTime.Now,
                IsCompleted = false
            };

            _context.TestSessions.Add(newSession);
            _context.SaveChanges();
            return newSession.SessionID;
        }

        // =========================================================================
        // МЕТОДИ ДЛЯ TEST PAGE
        // =========================================================================

        public void CompleteTestSession(int sessionID, List<UserAnswer> userAnswers)
        {
            _context.UserAnswers.AddRange(userAnswers);

            // Отримуємо всі питання, що належать цій сесії
            var allQuestions = _context.Questions
                .Where(q => userAnswers.Select(ua => ua.QuestionID).Contains(q.QuestionID))
                .ToList();

            decimal maxScore = allQuestions.Sum(q => (decimal?)q.Weight) ?? 0m;
            decimal finalScore = CalculateFinalScore(userAnswers);

            var session = _context.TestSessions.FirstOrDefault(s => s.SessionID == sessionID);
            if (session != null)
            {
                session.EndTime = DateTime.Now;
                session.Score = finalScore;
                session.MaxScore = maxScore;
                session.IsCompleted = true;

                _context.SaveChanges();

                MessageBox.Show($"Тестування успішно завершено! Ваш результат: {finalScore} з {maxScore} балів.",
                    "Результат тестування", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // =========================================================================
        // РОЗРАХУНОК РЕЗУЛЬТАТУ
        // =========================================================================

        private decimal CalculateFinalScore(List<UserAnswer> userAnswers)
        {
            decimal totalScore = 0m;

            var questionIds = userAnswers.Select(ua => ua.QuestionID).Distinct().ToList();
            var questions = _context.Questions
                .Include(q => q.Answers)
                .Where(q => questionIds.Contains(q.QuestionID))
                .ToList();

            foreach (var question in questions)
            {
                var userSelectedIds = userAnswers
                    .Where(ua => ua.QuestionID == question.QuestionID)
                    .Select(ua => ua.AnswerID)
                    .ToList();

                var correctAnswerIds = question.Answers
                    .Where(a => a.IsCorrect)
                    .Select(a => a.AnswerID)
                    .ToList();

                bool isMultiple = question.QuestionType == "Multiple";

                if (!isMultiple)
                {
                    // Single: правильна лише одна відповідь
                    if (userSelectedIds.Count == 1 && correctAnswerIds.Contains(userSelectedIds.First()))
                        totalScore += question.Weight;
                }
                else
                {
                    // Multiple: усі правильні вибрані, жодної зайвої
                    bool allCorrect = !correctAnswerIds.Except(userSelectedIds).Any();
                    bool noExtra = !userSelectedIds.Except(correctAnswerIds).Any();

                    if (allCorrect && noExtra)
                        totalScore += question.Weight;
                }
            }

            return totalScore;
        }

        // =========================================================================
        // ДОПОМОЖНІ МЕТОДИ
        // =========================================================================

        public User? GetUserByLogin(string login)
        {
            return _context.Users.FirstOrDefault(u => u.Login == login);
        }

        public List<Question> GetTestQuestionsBySession(int sessionID)
        {
            var testId = _context.TestSessions
                .Where(s => s.SessionID == sessionID)
                .Select(s => s.TestID)
                .FirstOrDefault();

            if (testId == 0) return new List<Question>();

            // Включаємо відповіді та сортуємо їх
            return _context.Questions
                .Where(q => q.TestID == testId)
                .Include(q => q.Answers)
                .OrderBy(q => q.SortOrder)
                .ToList();
        }

    }
}
