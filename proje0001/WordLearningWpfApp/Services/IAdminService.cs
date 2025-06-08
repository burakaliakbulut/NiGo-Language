using System.Collections.Generic;
using System.Threading.Tasks;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public interface IAdminService
    {
        Task<bool> ActivateUserAsync(string userId);
        Task<bool> DeactivateUserAsync(string userId);
        Task<Statistics> GetSystemStatisticsAsync();
        Task<Dictionary<string, int>> GetUserActivityStatsAsync();
        Task<Dictionary<string, int>> GetWordUsageStatsAsync();
        Task<Dictionary<string, double>> GetCategorySuccessRatesAsync();
        Task<Dictionary<string, int>> GetUserRegistrationStatsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetExamCompletionStatsAsync();
        Task<Dictionary<string, double>> GetAverageScoresByCategoryAsync();
        Task<bool> BackupDatabaseAsync(string backupPath);
        Task<bool> RestoreDatabaseAsync(string backupPath);
        Task<bool> ExportDataAsync(string exportPath, ExportFormat format);
        Task<bool> ImportDataAsync(string importPath, ImportFormat format);
    }
} 