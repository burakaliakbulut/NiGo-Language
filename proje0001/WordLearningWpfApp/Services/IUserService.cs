using System.Collections.Generic;
using System.Threading.Tasks;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> UpdateUserSettingsAsync(string userId, UserSettings settings);
        Task<UserSettings> GetUserSettingsAsync(string userId);
        Task<UserProfile> GetUserProfileAsync(string userId);
        Task<bool> UpdateUserProfileAsync(string userId, UserProfile profile);
        Task<UserPreferences> GetUserPreferencesAsync(string userId);
        Task<bool> UpdateUserPreferencesAsync(string userId, UserPreferences preferences);
        Task<Dictionary<string, long>> GetUserRoleDistributionAsync();
        Task<bool> IsAdminAsync(string userId);
    }
} 