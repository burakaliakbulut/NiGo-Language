using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Proje0001.Models;
using Proje0001.Services;

namespace Proje0001.ViewModels
{
    public class ProgressViewModel : BaseViewModel
    {
        private readonly IWordService _wordService;
        private readonly IStatisticsService _statisticsService;
        private readonly ICurrentUser _currentUser;

        public ObservableCollection<Word> LearnedWords { get; }
        public ObservableCollection<Word> LearningWords { get; }
        public ObservableCollection<Word> DifficultWords { get; }
        public UserStatistics Statistics { get; private set; }
        public ObservableCollection<CategoryProgress> CategoryProgress { get; }
        public ObservableCollection<DifficultyProgress> DifficultyProgress { get; }
        public ObservableCollection<DailyProgress> DailyProgress { get; }

        public ProgressViewModel(
            IWordService wordService,
            IStatisticsService statisticsService,
            ICurrentUser currentUser,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            // Initialize collections
            LearnedWords = new ObservableCollection<Word>();
            LearningWords = new ObservableCollection<Word>();
            DifficultWords = new ObservableCollection<Word>();
            CategoryProgress = new ObservableCollection<CategoryProgress>();
            DifficultyProgress = new ObservableCollection<DifficultyProgress>();
            DailyProgress = new ObservableCollection<DailyProgress>();
            Statistics = new UserStatistics();
        }

        public override async Task InitializeAsync()
        {
            try
            {
                IsBusy = true;
                await LoadProgressDataAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Failed to initialize progress view", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadProgressDataAsync()
        {
            if (string.IsNullOrEmpty(_currentUser.Id))
            {
                throw new InvalidOperationException("Current user ID is not available");
            }

            // Clear existing collections
            LearnedWords.Clear();
            LearningWords.Clear();
            DifficultWords.Clear();
            CategoryProgress.Clear();
            DifficultyProgress.Clear();
            DailyProgress.Clear();

            // Load words
            var words = await _wordService.GetAllWordsAsync(_currentUser.Id);
            if (words == null)
            {
                throw new InvalidOperationException("Failed to retrieve words");
            }

            // Populate word collections
            foreach (var word in words)
            {
                switch (word.Status)
                {
                    case WordStatus.Learned:
                        LearnedWords.Add(word);
                        break;
                    case WordStatus.Learning:
                        LearningWords.Add(word);
                        break;
                    case WordStatus.Difficult:
                        DifficultWords.Add(word);
                        break;
                }
            }

            // Load statistics
            Statistics = await _statisticsService.GetUserStatisticsAsync(_currentUser.Id);
            if (Statistics == null)
            {
                throw new InvalidOperationException("Failed to retrieve statistics");
            }

            // Update progress collections
            await UpdateProgressCollectionsAsync();

            // Notify property changes
            OnPropertyChanged(nameof(LearnedWords));
            OnPropertyChanged(nameof(LearningWords));
            OnPropertyChanged(nameof(DifficultWords));
            OnPropertyChanged(nameof(Statistics));
            OnPropertyChanged(nameof(CategoryProgress));
            OnPropertyChanged(nameof(DifficultyProgress));
            OnPropertyChanged(nameof(DailyProgress));
        }

        private async Task UpdateProgressCollectionsAsync()
        {
            try
            {
                // Update category progress
                var categories = await _wordService.GetCategoriesAsync();
                foreach (var category in categories)
                {
                    var progress = new CategoryProgress
                    {
                        Category = category,
                        LearnedCount = LearnedWords.Count(w => w.Category == category),
                        LearningCount = LearningWords.Count(w => w.Category == category),
                        DifficultCount = DifficultWords.Count(w => w.Category == category)
                    };
                    CategoryProgress.Add(progress);
                }

                // Update difficulty progress
                foreach (WordDifficulty difficulty in Enum.GetValues(typeof(WordDifficulty)))
                {
                    var progress = new DifficultyProgress
                    {
                        Difficulty = difficulty,
                        LearnedCount = LearnedWords.Count(w => w.DifficultyLevel == difficulty),
                        LearningCount = LearningWords.Count(w => w.DifficultyLevel == difficulty),
                        DifficultCount = DifficultWords.Count(w => w.DifficultyLevel == difficulty)
                    };
                    DifficultyProgress.Add(progress);
                }

                // Update daily progress
                var dailyStats = await _statisticsService.GetDailyProgressAsync(_currentUser.Id);
                foreach (var stat in dailyStats)
                {
                    DailyProgress.Add(new DailyProgress
                    {
                        Date = stat.Date,
                        WordsLearned = stat.WordsLearned,
                        WordsReviewed = stat.WordsReviewed,
                        SuccessRate = stat.SuccessRate
                    });
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Failed to update progress collections", ex);
                throw;
            }
        }

        public async Task RefreshProgressAsync()
        {
            try
            {
                IsBusy = true;
                await LoadProgressDataAsync();
                await ShowSuccessAsync("Progress refreshed successfully");
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Failed to refresh progress", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 