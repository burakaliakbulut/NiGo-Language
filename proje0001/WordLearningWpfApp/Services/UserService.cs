using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<UserPreferences> _preferences;
        private readonly IMongoCollection<UserWordProgress> _wordProgress;

        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("users");
            _preferences = database.GetCollection<UserPreferences>("user_preferences");
            _wordProgress = database.GetCollection<UserWordProgress>("user_word_progress");
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            try
            {
                await _users.InsertOneAsync(user);
                await CreateDefaultPreferencesAsync(user.Id);
                return user;
            }
            catch
            {
                throw new Exception("Failed to create user");
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            try
            {
                var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
                if (result.ModifiedCount > 0)
                {
                    return user;
                }
                throw new Exception("Failed to update user");
            }
            catch
            {
                throw new Exception("Failed to update user");
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var result = await _users.DeleteOneAsync(u => u.Id == userId);
                if (result.DeletedCount > 0)
                {
                    await _preferences.DeleteManyAsync(p => p.UserId == userId);
                    await _wordProgress.DeleteManyAsync(p => p.UserId == userId);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var update = Builders<User>.Update
                .Set(u => u.IsActive, false)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var update = Builders<User>.Update
                .Set(u => u.IsActive, true)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences)
        {
            try
            {
                var update = Builders<User>.Update
                    .Set(u => u.Preferences, preferences);

                var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, UserRole role)
        {
            var update = Builders<User>.Update
                .Set(u => u.Role, role)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _users.Find(u => u.Role == role).ToListAsync();
        }

        public async Task<List<User>> GetActiveUsersAsync()
        {
            return await _users.Find(u => u.IsActive).ToListAsync();
        }

        public async Task<List<User>> GetInactiveUsersAsync()
        {
            return await _users.Find(u => !u.IsActive).ToListAsync();
        }

        public async Task<long> GetUserCountAsync()
        {
            return await _users.CountDocumentsAsync(_ => true);
        }

        public async Task<long> GetActiveUserCountAsync()
        {
            return await _users.CountDocumentsAsync(u => u.IsActive);
        }

        public async Task<long> GetInactiveUserCountAsync()
        {
            return await _users.CountDocumentsAsync(u => !u.IsActive);
        }

        public async Task<Dictionary<string, long>> GetUserCountByRoleAsync()
        {
            var result = await _users.Aggregate()
                .Group(u => u.Role, g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            return result.ToDictionary(x => x.Role, x => x.Count);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateLastLoginAsync(string userId)
        {
            var update = Builders<User>.Update
                .Set(u => u.LastLoginAt, DateTime.UtcNow)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateDailyWordLimitAsync(string userId, int limit)
        {
            var update = Builders<User>.Update
                .Set(u => u.DailyWordLimit, limit)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateDefaultDifficultyLevelAsync(string userId, int level)
        {
            var update = Builders<User>.Update
                .Set(u => u.DefaultDifficultyLevel, level)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null) return false;

            user.PasswordHash = HashPassword(newPassword);
            await UpdateUserAsync(user);
            return true;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                throw new Exception("Kullanıcı bulunamadı.");
            if (!VerifyPassword(password, user.PasswordHash))
                throw new Exception("Şifre hatalı.");
            await UpdateLastLoginAsync(user.Id);
            return user;
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                throw new Exception("Kullanıcı bulunamadı.");
            // Gerçek uygulamada burada e-posta gönderimi yapılır.
            // Şimdilik mock olarak sadece Task.Delay ile simüle edelim.
            await Task.Delay(500);
        }

        public async Task LogoutAsync(string userId)
        {
            // Gerçek uygulamada oturum sonlandırma işlemleri yapılabilir.
            // Şimdilik mock olarak sadece Task.Delay ile simüle edelim.
            await Task.Delay(100);
        }

        public async Task<bool> UpdateUserProfileAsync(string userId, UserProfile profile)
        {
            var update = Builders<User>.Update
                .Set(u => u.Profile, profile)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<UserPreferences> GetUserPreferencesAsync(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            return user?.Preferences ?? new UserPreferences();
        }

        public async Task<UserProfile> GetUserProfileAsync(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            return user?.Profile ?? new UserProfile();
        }

        public async Task<bool> UpdateUserSettingsAsync(string userId, UserSettings settings)
        {
            var update = Builders<User>.Update
                .Set(u => u.Settings, settings)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<UserSettings> GetUserSettingsAsync(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            return user?.Settings ?? new UserSettings();
        }

        public async Task<List<User>> SearchUsersAsync(string searchTerm)
        {
            var filter = Builders<User>.Filter.Or(
                Builders<User>.Filter.Regex(u => u.Username, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                Builders<User>.Filter.Regex(u => u.Email, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
            );

            return await _users.Find(filter).ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetUserStatisticsAsync()
        {
            var totalUsers = await _users.CountDocumentsAsync(_ => true);
            var activeUsers = await _users.CountDocumentsAsync(u => u.IsActive);
            var adminUsers = await _users.CountDocumentsAsync(u => u.Role == "Admin");
            var regularUsers = await _users.CountDocumentsAsync(u => u.Role == "User");

            return new Dictionary<string, int>
            {
                ["TotalUsers"] = (int)totalUsers,
                ["ActiveUsers"] = (int)activeUsers,
                ["AdminUsers"] = (int)adminUsers,
                ["RegularUsers"] = (int)regularUsers
            };
        }

        public async Task<Dictionary<string, long>> GetUserRoleDistributionAsync()
        {
            var result = await _users.Aggregate()
                .Group(u => u.Role.ToString(), g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            return result.ToDictionary(x => x.Role, x => x.Count);
        }

        public async Task<bool> IsAdminAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            return user?.Role == UserRole.Admin;
        }

        public async Task<bool> UpdateUserStreakAsync(string userId, int streak)
        {
            try
            {
                var update = Builders<User>.Update
                    .Set(u => u.CurrentStreak, streak)
                    .Set(u => u.LastLoginDate, DateTime.UtcNow);

                var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserStatisticsAsync(string userId, UserStatistics statistics)
        {
            try
            {
                var update = Builders<User>.Update
                    .Set(u => u.Statistics, statistics);

                var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
                return result.ModifiedCount > 0;
            }
            catch
            {
                return false;
            }
        }
    }
} 