using System.Threading.Tasks;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Interfaces
{
    public interface INavigationService
    {
        Task NavigateToLoginAsync();
        Task NavigateToRegisterAsync();
        Task NavigateToMainAsync();
        Task NavigateToDictionaryAsync();
        Task NavigateToAddWordAsync();
        Task NavigateToEditWordAsync(Word word);
        Task NavigateToWordListAsync();
        Task NavigateToStatisticsAsync();
        Task NavigateToSettingsAsync();
        Task NavigateToForgotPasswordAsync();
        Task NavigateToDailyWordsAsync();
        Task NavigateToExamAsync();
        Task NavigateToProgressAsync();
        Task NavigateToWordStatsAsync(Word word);
        Task NavigateBackAsync();
    }
} 