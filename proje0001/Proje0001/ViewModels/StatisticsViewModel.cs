using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Proje0001.Services;
using Proje0001.Models;
using Proje0001.Views;

namespace Proje0001.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ICurrentUser _currentUser;
        private readonly IWordService _wordService;

        public ObservableCollection<DailyActivity> DailyActivity { get; private set; }
        public ObservableCollection<CategorySuccessRate> CategorySuccessRates { get; private set; }
        public ObservableCollection<WordStatusDistribution> WordStatusDistribution { get; private set; }
        public SystemStatistics SystemStatistics { get; private set; }

        public StatisticsViewModel(
            IStatisticsService statisticsService,
            ICurrentUser currentUser,
            IWordService wordService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));

            // Initialize collections
            DailyActivity = new ObservableCollection<DailyActivity>();
            CategorySuccessRates = new ObservableCollection<CategorySuccessRate>();
            WordStatusDistribution = new ObservableCollection<WordStatusDistribution>();
            SystemStatistics = new SystemStatistics();
        }

        public override async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Failed to load statistics", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadStatisticsAsync()
        {
            if (string.IsNullOrEmpty(_currentUser.Id))
            {
                throw new InvalidOperationException("Current user ID is not available");
            }

            var statistics = await _statisticsService.GetUserStatisticsAsync(_currentUser.Id);
            if (statistics == null)
            {
                throw new InvalidOperationException("Failed to retrieve statistics");
            }

            // Clear existing collections
            DailyActivity.Clear();
            CategorySuccessRates.Clear();
            WordStatusDistribution.Clear();

            // Convert and populate daily activity data
            if (statistics.DailyActivity != null)
            {
                foreach (var activity in statistics.DailyActivity)
                {
                    DailyActivity.Add(new DailyActivity
                    {
                        Date = activity.Date,
                        WordCount = activity.WordCount
                    });
                }
            }

            // Convert and populate category success rates
            if (statistics.CategorySuccessRates != null)
            {
                foreach (var rate in statistics.CategorySuccessRates)
                {
                    CategorySuccessRates.Add(new CategorySuccessRate
                    {
                        Category = rate.Category,
                        SuccessRate = rate.SuccessRate
                    });
                }
            }

            // Convert and populate word status distribution
            if (statistics.WordStatusDistribution != null)
            {
                foreach (var distribution in statistics.WordStatusDistribution)
                {
                    WordStatusDistribution.Add(new WordStatusDistribution
                    {
                        Status = distribution.Status,
                        Count = distribution.Count
                    });
                }
            }

            // Update system statistics
            SystemStatistics = new SystemStatistics
            {
                TotalUsers = statistics.TotalUsers,
                TotalWords = statistics.TotalWords,
                AverageWordsPerUser = statistics.AverageWordsPerUser,
                MostCommonCategory = statistics.MostCommonCategory,
                MostDifficultWord = statistics.MostDifficultWord
            };

            // Notify property changes
            OnPropertyChanged(nameof(DailyActivity));
            OnPropertyChanged(nameof(CategorySuccessRates));
            OnPropertyChanged(nameof(WordStatusDistribution));
            OnPropertyChanged(nameof(SystemStatistics));
        }

        public async Task RefreshStatisticsAsync()
        {
            try
            {
                IsBusy = true;
                await LoadStatisticsAsync();
                await ShowSuccessAsync("Statistics refreshed successfully");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Failed to refresh statistics", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 