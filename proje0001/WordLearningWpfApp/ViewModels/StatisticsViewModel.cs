using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;

namespace WordLearningWpfApp.ViewModels
{
    public class StatisticsViewModel : ViewModelBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IAuthService _authService;
        private ObservableCollection<DailyActivity> _dailyActivities = new();
        private ObservableCollection<CategorySuccessRate> _categorySuccessRates = new();
        private ObservableCollection<WordStatusDistribution> _wordStatusDistribution = new();
        private Statistics? _statistics;
        private DateTime _startDate = DateTime.Today.AddDays(-30);
        private DateTime _endDate = DateTime.Today;
        private bool _isBusy;

        public StatisticsViewModel(
            IStatisticsService statisticsService,
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            LoadStatisticsCommand = new RelayCommand(async _ => await LoadStatisticsAsync());
            BackCommand = new RelayCommand(async _ => await BackAsync());

            LoadStatisticsAsync().ConfigureAwait(false);
        }

        public ObservableCollection<DailyActivity> DailyActivities
        {
            get => _dailyActivities;
            private set
            {
                _dailyActivities = value;
                OnPropertyChanged(nameof(DailyActivities));
            }
        }

        public ObservableCollection<CategorySuccessRate> CategorySuccessRates
        {
            get => _categorySuccessRates;
            private set
            {
                _categorySuccessRates = value;
                OnPropertyChanged(nameof(CategorySuccessRates));
            }
        }

        public ObservableCollection<WordStatusDistribution> WordStatusDistribution
        {
            get => _wordStatusDistribution;
            private set
            {
                _wordStatusDistribution = value;
                OnPropertyChanged(nameof(WordStatusDistribution));
            }
        }

        public Statistics? Statistics
        {
            get => _statistics;
            private set
            {
                _statistics = value;
                OnPropertyChanged(nameof(Statistics));
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public ICommand LoadStatisticsCommand { get; }
        public ICommand BackCommand { get; }

        private async Task LoadStatisticsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await ShowErrorAsync("User not found");
                    await _navigationService.NavigateToLoginAsync();
                    return;
                }

                var userStats = await _statisticsService.GetUserStatisticsByDateRangeAsync(currentUser.Id, StartDate, EndDate);
                var dailyActivity = await _statisticsService.GetDailyActivityAsync(currentUser.Id, 30);
                var categorySuccessRates = await _statisticsService.GetSuccessRatesByCategoryAsync(currentUser.Id);
                var wordStatusDistribution = await _statisticsService.GetWordStatusDistributionAsync(currentUser.Id);
                var systemStats = await _statisticsService.GetSystemStatisticsAsync();

                // Convert dictionary results to model types
                DailyActivities = new ObservableCollection<DailyActivity>(
                    dailyActivity.Select(kvp => new DailyActivity
                    {
                        Date = kvp.Key,
                        Count = kvp.Value
                    }));

                CategorySuccessRates = new ObservableCollection<CategorySuccessRate>(
                    categorySuccessRates.Select(kvp => new CategorySuccessRate
                    {
                        Category = kvp.Key,
                        Rate = kvp.Value
                    }));

                WordStatusDistribution = new ObservableCollection<WordStatusDistribution>(
                    wordStatusDistribution.Select(kvp => new WordStatusDistribution
                    {
                        WordStatus = kvp.Key,
                        WordCount = kvp.Value
                    }));

                Statistics = systemStats;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading statistics", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task BackAsync()
        {
            await _navigationService.NavigateToMainAsync();
        }
    }
} 