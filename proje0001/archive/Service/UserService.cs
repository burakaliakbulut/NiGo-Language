using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Bson;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Data;
using MongoDB.Driver;
using System.Collections.Generic;
using System;

namespace WordLearningWpfApp.Service
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(MongoDbContext dbContext)
        {
            _users = dbContext.Users;
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<bool> AddUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return true;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var result = await _users.DeleteOneAsync(u => u.Id == id);
            return result.DeletedCount > 0;
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<User> RegisterAsync(string username, string email, string password)
        {
            var existingUser = await GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                throw new Exception("Kullanıcı adı zaten kullanılıyor.");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                IsActive = true
            };

            await AddUserAsync(user);
            return user;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
                if (user != null && user.PasswordHash == HashPassword(password))
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
                    return user;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Giriş işlemi sırasında bir hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task<bool> ResetPasswordAsync(string userName, string newPassword)
        {
            var user = await GetUserByUsernameAsync(userName);
            if (user == null) return false;

            user.PasswordHash = HashPassword(newPassword);
            await UpdateUserAsync(user);
            return true;
        }

        public async Task<bool> UpdateDailyWordLimitAsync(ObjectId userId, int newLimit)
        {
            if (newLimit < 1) return false;
            var user = await GetUserByIdAsync(userId.ToString());
            if (user == null) return false;

            user.DailyWordLimit = newLimit;
            await UpdateUserAsync(user);
            return true;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;

            // TODO: Implement actual email sending logic
            // For now, just return true to simulate success
            return true;
        }

        public async Task<bool> RegisterUserAsync(User user)
        {
            try
            {
                // Check if user already exists
                var existingUser = await GetUserByEmailAsync(user.Email);
                if (existingUser != null)
                    return false;

                // Hash the password
                user.PasswordHash = HashPassword(user.PasswordHash);

                // Set creation date
                user.CreatedAt = DateTime.UtcNow;

                await _users.InsertOneAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AuthenticateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;

            return user.PasswordHash == HashPassword(password);
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
                if (user != null && user.PasswordHash == HashPassword(password)) // In a real app, use proper password hashing
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
                    return user;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> RegisterAsync(User user)
        {
            try
            {
                var existingUser = await _users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    throw new Exception("Bu e-posta adresi zaten kullanılıyor.");
                }

                user.CreatedAt = DateTime.UtcNow;
                user.LastLoginAt = DateTime.UtcNow;
                await _users.InsertOneAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Kayıt işlemi sırasında bir hata oluştu: {ex.Message}", ex);
            }
        }
    }
} 