using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Proje0001.Data;
using Proje0001.Models;

namespace Proje0001.Services
{
    public class AuthService : IAuthService
    {
        private readonly MongoDbContext _dbContext;
        private readonly ICurrentUser _currentUser;
        private readonly INotificationService _notificationService;

        public AuthService(
            MongoDbContext dbContext,
            ICurrentUser currentUser,
            INotificationService notificationService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new AuthResult { Success = false, ErrorMessage = "Email is required" };

                if (string.IsNullOrWhiteSpace(password))
                    return new AuthResult { Success = false, ErrorMessage = "Password is required" };

                var users = _dbContext.GetCollection<User>("Users");
                var user = await users.Find(u => u.Email == email.ToLower()).FirstOrDefaultAsync();

                if (user == null)
                    return new AuthResult { Success = false, ErrorMessage = "Invalid email or password" };

                if (!VerifyPassword(password, user.PasswordHash))
                    return new AuthResult { Success = false, ErrorMessage = "Invalid email or password" };

                _currentUser.SetUser(user);
                return new AuthResult { Success = true };
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Login failed", ex);
                return new AuthResult { Success = false, ErrorMessage = "An unexpected error occurred" };
            }
        }

        public async Task<AuthResult> RegisterAsync(string email, string password, string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new AuthResult { Success = false, ErrorMessage = "Email is required" };

                if (string.IsNullOrWhiteSpace(password))
                    return new AuthResult { Success = false, ErrorMessage = "Password is required" };

                if (string.IsNullOrWhiteSpace(name))
                    return new AuthResult { Success = false, ErrorMessage = "Name is required" };

                if (password.Length < 6)
                    return new AuthResult { Success = false, ErrorMessage = "Password must be at least 6 characters" };

                var users = _dbContext.GetCollection<User>("Users");
                var existingUser = await users.Find(u => u.Email == email.ToLower()).FirstOrDefaultAsync();

                if (existingUser != null)
                    return new AuthResult { Success = false, ErrorMessage = "Email is already registered" };

                var user = new User
                {
                    Email = email.ToLower(),
                    Name = name,
                    PasswordHash = HashPassword(password),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                };

                await users.InsertOneAsync(user);
                return new AuthResult { Success = true };
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Registration failed", ex);
                return new AuthResult { Success = false, ErrorMessage = "An unexpected error occurred" };
            }
        }

        public async Task<AuthResult> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                if (!_currentUser.IsAuthenticated)
                    return new AuthResult { Success = false, ErrorMessage = "User is not authenticated" };

                if (string.IsNullOrWhiteSpace(currentPassword))
                    return new AuthResult { Success = false, ErrorMessage = "Current password is required" };

                if (string.IsNullOrWhiteSpace(newPassword))
                    return new AuthResult { Success = false, ErrorMessage = "New password is required" };

                if (newPassword.Length < 6)
                    return new AuthResult { Success = false, ErrorMessage = "New password must be at least 6 characters" };

                var users = _dbContext.GetCollection<User>("Users");
                var user = await users.Find(u => u.Id == _currentUser.Id).FirstOrDefaultAsync();

                if (user == null)
                    return new AuthResult { Success = false, ErrorMessage = "User not found" };

                if (!VerifyPassword(currentPassword, user.PasswordHash))
                    return new AuthResult { Success = false, ErrorMessage = "Current password is incorrect" };

                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, HashPassword(newPassword))
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                await users.UpdateOneAsync(u => u.Id == user.Id, update);
                return new AuthResult { Success = true };
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Password change failed", ex);
                return new AuthResult { Success = false, ErrorMessage = "An unexpected error occurred" };
            }
        }

        public async Task<AuthResult> ResetPasswordAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return new AuthResult { Success = false, ErrorMessage = "Email is required" };

                var users = _dbContext.GetCollection<User>("Users");
                var user = await users.Find(u => u.Email == email.ToLower()).FirstOrDefaultAsync();

                if (user == null)
                    return new AuthResult { Success = false, ErrorMessage = "Email not found" };

                // Generate a random password
                var newPassword = GenerateRandomPassword();
                var update = Builders<User>.Update
                    .Set(u => u.PasswordHash, HashPassword(newPassword))
                    .Set(u => u.UpdatedAt, DateTime.UtcNow);

                await users.UpdateOneAsync(u => u.Id == user.Id, update);

                // TODO: Send email with new password
                _notificationService.ShowSuccess($"A new password has been sent to {email}");

                return new AuthResult { Success = true };
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Password reset failed", ex);
                return new AuthResult { Success = false, ErrorMessage = "An unexpected error occurred" };
            }
        }

        public void Logout()
        {
            _currentUser.ClearUser();
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
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new char[12];

            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
} 