using System.Threading.Tasks;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public interface IAuthService
    {
        Task<User> LoginAsync(string email, string password);
        Task<User> RegisterAsync(User user, string password);
        Task<User?> GetCurrentUserAsync();
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> ValidateTokenAsync(string token);
        Task LogoutAsync();
        Task<bool> SaveCredentialsAsync(string email, string password);
        Task<bool> IsAuthenticatedAsync();
        Task<bool> IsEmailVerifiedAsync(string email);
        Task<bool> VerifyEmailAsync(string email, string token);
    }
} 