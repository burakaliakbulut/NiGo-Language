using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Models;
using WordLearningWpfApp.Services;
using WordLearningWpfApp.Views;

namespace WordLearningWpfApp.ViewModels
{
    public class ExamViewModel : ViewModelBase
    {
        private readonly IExamService _examService;
        private readonly IAuthService _authService;
        private readonly IStatisticsService _statisticsService;
        private ObservableCollection<Question> _questions = new();
        private Question? _currentQuestion;
        private ExamResult? _examResult;
        private int _currentQuestionIndex;
        private int _correctAnswers;
        private int _totalQuestions;
        private bool _isExamCompleted;
        private bool _isBusy;
        private int _score;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Question> Questions
        {
            get => _questions;
            set
            {
                _questions = value;
                OnPropertyChanged();
            }
        }

        public Question? CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                _currentQuestion = value;
                OnPropertyChanged();
            }
        }

        public ExamResult? ExamResult
        {
            get => _examResult;
            private set
            {
                _examResult = value;
                OnPropertyChanged(nameof(ExamResult));
            }
        }

        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                _currentQuestionIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public int CorrectAnswers
        {
            get => _correctAnswers;
            private set
            {
                _correctAnswers = value;
                OnPropertyChanged(nameof(CorrectAnswers));
            }
        }

        public int TotalQuestions
        {
            get => _totalQuestions;
            set
            {
                _totalQuestions = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public bool IsExamCompleted
        {
            get => _isExamCompleted;
            private set
            {
                _isExamCompleted = value;
                OnPropertyChanged(nameof(IsExamCompleted));
            }
        }

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScoreText));
            }
        }

        public string ProgressText => $"Question {CurrentQuestionIndex + 1} of {TotalQuestions}";
        public string ScoreText => $"Score: {Score}/{TotalQuestions}";

        public ICommand StartExamCommand { get; }
        public ICommand AnswerCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand FinishExamCommand { get; }

        public ExamViewModel(
            IExamService examService,
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService,
            IStatisticsService statisticsService)
            : base(navigationService, notificationService)
        {
            _examService = examService ?? throw new ArgumentNullException(nameof(examService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));

            StartExamCommand = new RelayCommand(async _ => await StartExamAsync());
            AnswerCommand = new RelayCommand(async _ => await AnswerQuestionAsync(), _ => CurrentQuestion != null && !IsExamCompleted);
            BackCommand = new RelayCommand(async _ => await BackAsync());
            NextQuestionCommand = new RelayCommand(async () => await MoveToNextQuestionAsync());
            FinishExamCommand = new RelayCommand(async () => await FinishExamAsync());

            StartExamAsync().ConfigureAwait(false);
        }

        private async Task StartExamAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsExamCompleted = false;
                ExamResult = null;
                CorrectAnswers = 0;
                CurrentQuestionIndex = 0;
                Score = 0;

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await ShowErrorAsync("User not found");
                    await _navigationService.NavigateToLoginAsync();
                    return;
                }

                var questions = await _examService.GenerateExamAsync(currentUser.Id, ExamType.Mixed, 10);
                if (questions.Count == 0)
                {
                    await ShowErrorAsync("No questions available for exam");
                    return;
                }

                Questions = new ObservableCollection<Question>(questions);
                TotalQuestions = questions.Count;
                CurrentQuestion = questions[0];
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error starting exam", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AnswerQuestionAsync()
        {
            if (CurrentQuestion == null || IsExamCompleted) return;

            try
            {
                if (CurrentQuestion.SelectedAnswer == null)
                {
                    await ShowErrorAsync("Please select an answer");
                    return;
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await ShowErrorAsync("User not found");
                    await _navigationService.NavigateToLoginAsync();
                    return;
                }

                var isCorrect = await _examService.CheckAnswerAsync(currentUser.Id, CurrentQuestion.Id, CurrentQuestion.SelectedAnswer);
                if (isCorrect)
                {
                    CorrectAnswers++;
                    Score++;
                }

                var statistics = await _statisticsService.GetUserStatisticsAsync(currentUser.Id);
                statistics.TotalQuestionsAnswered++;
                if (isCorrect)
                {
                    statistics.CorrectAnswers++;
                }
                await _statisticsService.UpdateStatisticsAsync(currentUser.Id, statistics);

                CurrentQuestionIndex++;
                if (CurrentQuestionIndex >= TotalQuestions)
                {
                    await CompleteExamAsync();
                }
                else
                {
                    CurrentQuestion = Questions[CurrentQuestionIndex];
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error checking answer", ex);
            }
        }

        private async Task CompleteExamAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    await ShowErrorAsync("User not found");
                    await _navigationService.NavigateToLoginAsync();
                    return;
                }

                var result = await _examService.CompleteExamAsync(currentUser.Id, CorrectAnswers, TotalQuestions);
                ExamResult = result;
                IsExamCompleted = true;
                await ShowSuccessAsync($"Exam completed! Your score: {Score}/{TotalQuestions}");
                await _navigationService.NavigateToMainAsync();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error completing exam", ex);
            }
        }

        private async Task BackAsync()
        {
            await _navigationService.NavigateToMainAsync();
        }

        private async Task MoveToNextQuestionAsync()
        {
            if (CurrentQuestionIndex < TotalQuestions - 1)
            {
                CurrentQuestionIndex++;
                CurrentQuestion = Questions[CurrentQuestionIndex];
            }
        }

        private async Task FinishExamAsync()
        {
            await CompleteExamAsync();
        }
    }
} 