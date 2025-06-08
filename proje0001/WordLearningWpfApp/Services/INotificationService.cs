using System;
using System.Threading.Tasks;

namespace WordLearningWpfApp.Services
{
    public interface INotificationService
    {
        Task ShowSuccessAsync(string message);
        Task ShowErrorAsync(string message);
        Task ShowErrorAsync(string message, Exception ex);
        Task ShowWarningAsync(string message);
        Task ShowInfoAsync(string message);
        Task<bool> ShowConfirmationAsync(string message);
        void ShowSuccess(string message);
        void ShowError(string message);
        void ShowWarning(string message);
        void ShowInfo(string message);
        bool ShowConfirmation(string message);
        Task ShowSuccessAsync(string title, string message);
        Task<bool> ShowConfirmationAsync(string title, string message);
    }
} 