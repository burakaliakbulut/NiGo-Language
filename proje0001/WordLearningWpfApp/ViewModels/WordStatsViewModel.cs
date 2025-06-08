using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;

namespace WordLearningWpfApp.ViewModels
{
    public class WordStatsViewModel : ViewModelBase
    {
        private readonly IStatisticsService _statisticsService;
        protected new readonly INavigationService _navigationService;
        protected new readonly INotificationService _notificationService;
        private ObservableCollection<WordStatistics> _wordStats;
        private bool _isLoading;

        public ObservableCollection<WordStatistics> WordStats
        {
            get => _wordStats;
            set => SetProperty(ref _wordStats, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }

        public WordStatsViewModel(
            IStatisticsService statisticsService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            WordStats = new ObservableCollection<WordStatistics>();
            BackCommand = new RelayCommand(async () => await ExecuteBackAsync());
            RefreshCommand = new RelayCommand(async () => await LoadWordStatsAsync());
            _ = InitializeAsync();
        }

        public override async Task InitializeAsync()
        {
            await LoadWordStatsAsync();
        }

        private async Task LoadWordStatsAsync()
        {
            try
            {
                IsLoading = true;
                var stats = await _statisticsService.GetWordStatisticsAsync();
                WordStats.Clear();
                foreach (var stat in stats)
                {
                    WordStats.Add(stat);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Failed to load word statistics", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExecuteBackAsync()
        {
            // TODO: Implement navigation
        }
    }
} 