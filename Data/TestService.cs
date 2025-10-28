using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TestSystem.Models;

namespace TestSystem.Data
{
    public class TestService
    {
        // Отримати всі тести викладача
        public static List<Test> GetTeacherTests(int teacherId)
        {
            using var db = new TestSystemContext();
            return db.Tests
                .Include(t => t.Questions)
                .Where(t => t.CreatedBy == teacherId)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();
        }

        // Створити новий тест
        public static int CreateTest(Test test)
        {
            using var db = new TestSystemContext();
            db.Tests.Add(test);
            db.SaveChanges();
            return test.TestID;
        }

        // Оновити тест
        public static bool UpdateTest(Test test)
        {
            using var db = new TestSystemContext();
            var existingTest = db.Tests.FirstOrDefault(t => t.TestID == test.TestID);
            if (existingTest == null) return false;

            existingTest.TestName = test.TestName;
            existingTest.Description = test.Description;
            existingTest.TimeLimit = test.TimeLimit;
            existingTest.MaxAttempts = test.MaxAttempts;
            existingTest.IsActive = test.IsActive;

            db.SaveChanges();
            return true;
        }

        // Видалити тест
        public static bool DeleteTest(int testId)
        {
            using var db = new TestSystemContext();
            var test = db.Tests.FirstOrDefault(t => t.TestID == testId);
            if (test == null) return false;

            db.Tests.Remove(test);
            db.SaveChanges();
            return true;
        }

        //Завантаження тесту з усіма питаннями та відповідями
        public static Test GetTestWithDetails(int testId)
        {
            using var db = new TestSystemContext();
            return db.Tests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefault(t => t.TestID == testId);
        }


        public static void UpdateTestDetails(Test updatedTest, List<Question> updatedQuestions)
        {
            using var db = new TestSystemContext();

            // 1. Прикріплюємо основний тест (EntityState.Modified)
            db.Tests.Attach(updatedTest).State = EntityState.Modified;

            // 2. Отримуємо поточний список ID питань, які є в БД
            var existingQuestionIds = db.Questions
                .Where(q => q.TestID == updatedTest.TestID)
                .Select(q => q.QuestionID)
                .ToList();

            // 3. Обробка питань
            foreach (var question in updatedQuestions)
            {
                question.TestID = updatedTest.TestID;

                if (question.QuestionID == 0)
                {
                    db.Questions.Add(question);
                    if (question.Answers != null)
                    {
                        foreach (var answer in question.Answers)
                        {
                            db.Answers.Add(answer);
                        }
                    }
                }
                else
                {
                    // Існуюче питання: оновлюємо
                    db.Questions.Attach(question).State = EntityState.Modified;
                    existingQuestionIds.Remove(question.QuestionID);

                    // >>> ІНТЕГРАЦІЯ: Обробляємо відповіді для цього існуючого питання
                    UpdateAnswersForQuestion(db, question);
                }
            }

            // 4. Видалення питань, які були видалені з UI
            var questionsToDelete = db.Questions
                .Where(q => existingQuestionIds.Contains(q.QuestionID))
                .ToList();

            db.Questions.RemoveRange(questionsToDelete);

            // Зберігаємо всі зміни в одній транзакції!
            db.SaveChanges();
        }


        private static void UpdateAnswersForQuestion(TestSystemContext db, Question updatedQuestion)
        {
            // 1. Отримуємо поточні ID відповідей, які існують у базі даних
            var existingAnswerIds = db.Answers
                .Where(a => a.QuestionID == updatedQuestion.QuestionID)
                .Select(a => a.AnswerID)
                .ToList();

            // 2. Обробляємо кожну відповідь з оновленого списку (з UI)
            if (updatedQuestion.Answers != null)
            {
                foreach (var answer in updatedQuestion.Answers)
                {
                    answer.QuestionID = updatedQuestion.QuestionID;

                    if (answer.AnswerID == 0)
                    {
                        // Це нова відповідь (ID = 0): додаємо її
                        db.Answers.Add(answer);
                    }
                    else
                    {
                        // Це існуюча відповідь (ID > 0): оновлюємо її

                        // Перевіряємо, чи ця відповідь дійсно існувала в БД
                        if (existingAnswerIds.Contains(answer.AnswerID))
                        {
                            db.Answers.Attach(answer).State = EntityState.Modified;
                            existingAnswerIds.Remove(answer.AnswerID);
                        }
                        // Якщо ID існуючої відповіді не було в оригінальному списку, 
                        // це може бути помилка даних або ознака, що об'єкт уже був видалений.
                    }
                }
            }
            // 3. Видалення відповідей, яких немає в оновленому списку (залишилися в existingAnswerIds)
            if (existingAnswerIds.Any())
            {
                var answersToDelete = db.Answers
                    .Where(a => existingAnswerIds.Contains(a.AnswerID))
                    .ToList();

                db.Answers.RemoveRange(answersToDelete);
            }
        }

    }
}
