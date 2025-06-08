using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IUserService _userService;
        private User? _currentUser;

        public bool IsAuthenticated => _currentUser != null;

        public AuthService(MongoDbContext dbContext, IUserService userService)
        {
            _users = dbContext.GetCollection<User>(MongoDbContext.UsersCollection);
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            try
            {
                var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
                if (user == null || !VerifyPassword(password, user.PasswordHash))
                {
                    throw new InvalidOperationException("Invalid email or password");
                }

                if (!user.IsActive)
                {
                    throw new InvalidOperationException("User account is deactivated");
                }

                _currentUser = user;
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Login failed", ex);
            }
        }

        public async Task<User> RegisterAsync(User user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            try
            {
                if (await _users.Find(u => u.Email == user.Email).AnyAsync())
                {
                    throw new InvalidOperationException("Email already exists");
                }

                user.PasswordHash = HashPassword(password);
                user.CreatedAt = DateTime.UtcNow;
                user.IsActive = true;
                user.IsEmailVerified = false;

                await _users.InsertOneAsync(user);
                _currentUser = user;
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Registration failed", ex);
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            return _currentUser;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(currentPassword))
                throw new ArgumentException("Current password cannot be empty", nameof(currentPassword));
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("New password cannot be empty", nameof(newPassword));
            if (newPassword.Length < 8)
                throw new ArgumentException("New password must be at least 8 characters long", nameof(newPassword));

            try
            {
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null || !VerifyPassword(currentPassword, user.PasswordHash))
                {
                    return false;
                }

                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, HashPassword(newPassword))
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Password change failed", ex);
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            try
            {
                var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
                if (user == null)
                {
                    return false;
                }

                var newPassword = GenerateRandomPassword();
                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, HashPassword(newPassword))
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                var result = await _users.UpdateOneAsync(u => u.Id == user.Id, update);
                if (result.ModifiedCount > 0)
                {
                    // TODO: Send email with new password
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Password reset failed", ex);
            }
        }

        public async Task LogoutAsync()
        {
            _currentUser = null;
        }

        public async Task<bool> SaveCredentialsAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            try
            {
                var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
                if (user == null)
                {
                    return false;
                }

                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, HashPassword(password))
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                var result = await _users.UpdateOneAsync(u => u.Id == user.Id, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to save credentials", ex);
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            return _currentUser != null;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+";
            var random = new Random();
            var password = new char[12];

            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null)
            {
                return false;
            }

            // Generate a password reset token
            var resetToken = Guid.NewGuid().ToString("N");
            var update = Builders<User>.Update
                .Set(u => u.PasswordResetToken, resetToken)
                .Set(u => u.PasswordResetTokenExpiry, DateTime.UtcNow.AddHours(24))
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            await _users.UpdateOneAsync(u => u.Id == user.Id, update);
            // TODO: Send email with reset token
            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var user = await _users.Find(u => u.PasswordResetToken == token).FirstOrDefaultAsync();
            if (user == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> IsEmailVerifiedAsync(string email)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return user?.IsEmailVerified ?? false;
        }

        public async Task<bool> VerifyEmailAsync(string email, string token)
        {
            var user = await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (user == null || user.EmailVerificationToken != token)
            {
                return false;
            }

            var update = Builders<User>.Update
                .Set(u => u.IsEmailVerified, true)
                .Set(u => u.EmailVerificationToken, null)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Id == user.Id, update);
            return result.ModifiedCount > 0;
        }
    }
} 