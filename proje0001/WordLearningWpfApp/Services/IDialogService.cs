using System.Threading.Tasks;

namespace WordLearningWpfApp.Services
{
    public interface IDialogService
    {
        Task<bool> ShowDialogAsync(string title, string message);
        Task<bool> ShowConfirmationAsync(string title, string message);
        Task<string> ShowInputDialogAsync(string title, string message);
        Task<T> ShowCustomDialogAsync<T>(string title, object content);
    }
} 