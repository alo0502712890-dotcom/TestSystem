using System;
using System.Collections.Generic;
using System.Linq;
using TestSystem.Models;

namespace TestSystem.Data
{
    public class AuthStore
    {
        // Завантажує всіх користувачів (для адміністрування)
        public static List<User> LoadUsers()
        {
            using var db = new TestSystemContext();
            return db.Users.OrderBy(u => u.Login).ToList();
        }

        // Реєстрація нового користувача
        public static bool RegisterUser(string login, string password, string fullName, string email, string userType)
        {
            using var db = new TestSystemContext();

            // Перевірка унікальності логіну/email
            if (db.Users.Any(u => u.Login == login || u.Email == email))
                return false;

            string salt = PasswordHasher.GenerateSalt();
            string hash = PasswordHasher.HashPassword(password, salt);

            var user = new User
            {
                Login = login,
                PasswordHash = hash,
                Salt = salt,
                FullName = fullName,
                Email = email,
                UserType = userType,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            db.Users.Add(user);
            db.SaveChanges();
            return true;
        }

        // Вхід користувача 
        public static User? TrySignIn(string login, string password)
        {
            using var db = new TestSystemContext();

            var user = db.Users.FirstOrDefault(u => u.Login == login);
            if (user == null) return null;

            // Перевіряємо, чи активний користувач
            if (user.IsActive == false) return null;

            // Якщо пароль зберігається у відкритому вигляді (Salt == NULL)
            if (string.IsNullOrEmpty(user.Salt))
            {
                if (PasswordHasher.VerifyPlainText(password, user.PasswordHash))
                {
                    MigrateUserToNewHashing(user, password);
                    return user;
                }
            }
            // Якщо є сіль - використовуємо PBKDF2
            else if (!string.IsNullOrEmpty(user.Salt))
            {
                if (PasswordHasher.Verify(password, user.Salt, user.PasswordHash))
                    return user;
            }
            else
            // Якщо солі немає - перевіряємо старий SHA256
            {
                if (PasswordHasher.VerifyLegacySha256(password, user.PasswordHash))
                {
                    // Міграція на нову систему хешування
                    MigrateUserToNewHashing(user, password);
                    return user;
                }
            }

            return null;
        }

        // Допоміжний метод для міграції користувача на нову систему хешування
        private static void MigrateUserToNewHashing(User user, string password)
        {
            using var db = new TestSystemContext();
            var userToUpdate = db.Users.FirstOrDefault(u => u.UserId == user.UserId);
            if (userToUpdate != null)
            {
                string newSalt = PasswordHasher.GenerateSalt();
                string newHash = PasswordHasher.HashPassword(password, newSalt);

                userToUpdate.Salt = newSalt;
                userToUpdate.PasswordHash = newHash;
                db.SaveChanges();
            }
        }

        // Оновлення пароля користувача
        public static bool UpdatePassword(int userId, string newPassword)
        {
            using var db = new TestSystemContext();
            var user = db.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) return false;

            string newSalt = PasswordHasher.GenerateSalt();
            string newHash = PasswordHasher.HashPassword(newPassword, newSalt);

            user.Salt = newSalt;
            user.PasswordHash = newHash;
            db.SaveChanges();
            return true;
        }

        // Оновлення інших даних профілю
        public static void UpdateProfile(User updatedUser)
        {
            using var db = new TestSystemContext();
            var user = db.Users.FirstOrDefault(u => u.UserId == updatedUser.UserId);
            if (user == null) return;

            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;
            user.UserType = updatedUser.UserType;
            user.IsActive = updatedUser.IsActive;

            db.SaveChanges();
        }

    }
}