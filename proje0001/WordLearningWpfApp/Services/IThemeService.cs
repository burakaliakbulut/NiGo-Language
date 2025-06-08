using System.Threading.Tasks;

namespace WordLearningWpfApp.Services
{
    public interface IThemeService
    {
        Task SetThemeAsync(string themeName);
        Task<string> GetCurrentThemeAsync();
        Task<bool> IsDarkModeEnabledAsync();
        Task ToggleDarkModeAsync();
        Task ApplyThemeAsync(string themeName);
    }
} 