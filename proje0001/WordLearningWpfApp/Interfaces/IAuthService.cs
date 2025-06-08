using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Interfaces;

public interface IAuthService
{
    Task<User> LoginAsync(string email, string password);
    Task<User> RegisterAsync(string username, string email, string password);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(string email);
    Task<bool> ValidateTokenAsync(string token);
    Task LogoutAsync();
    User? GetCurrentUser();
} 