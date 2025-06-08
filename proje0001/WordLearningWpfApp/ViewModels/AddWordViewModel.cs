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
    public class AddWordViewModel : ViewModelBase
    {
        private readonly IWordService _wordService;
        private readonly IAuthService _authService;
        private string _wordText = string.Empty;
        private string _translation = string.Empty;
        private string _example = string.Empty;
        private string _notes = string.Empty;
        private DifficultyLevel _difficulty = DifficultyLevel.Medium;
        private string _category = string.Empty;
        private ObservableCollection<string> _categories = new();
        private bool _isBusy;

        public new bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string WordText
        {
            get => _wordText;
            set
            {
                _wordText = value;
                OnPropertyChanged(nameof(WordText));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Translation
        {
            get => _translation;
            set
            {
                _translation = value;
                OnPropertyChanged(nameof(Translation));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Example
        {
            get => _example;
            set
            {
                _example = value;
                OnPropertyChanged(nameof(Example));
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public DifficultyLevel Difficulty
        {
            get => _difficulty;
            set
            {
                _difficulty = value;
                OnPropertyChanged(nameof(Difficulty));
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            private set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddWordViewModel(
            IWordService wordService,
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            CancelCommand = new RelayCommand(async _ => await CancelAsync());

            LoadCategoriesAsync().ConfigureAwait(false);
        }

        private async Task LoadCategoriesAsync()
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

                var categories = await _wordService.GetCategoriesAsync(currentUser.Id);
                Categories = new ObservableCollection<string>(categories);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading categories", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSave()
        {
            return !IsBusy && 
                   !string.IsNullOrWhiteSpace(WordText) && 
                   !string.IsNullOrWhiteSpace(Translation) && 
                   !string.IsNullOrWhiteSpace(Category);
        }

        private async Task SaveAsync()
        {
            if (!CanSave()) return;

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

                var word = new Word
                {
                    Text = WordText,
                    Translation = Translation,
                    Example = Example,
                    Notes = Notes,
                    Difficulty = Difficulty,
                    Category = Category,
                    UserId = currentUser.Id
                };

                var result = await _wordService.AddWordAsync(currentUser.Id, word);
                if (result)
                {
                    await ShowSuccessAsync("Word added successfully");
                    await _navigationService.NavigateToWordListAsync();
                }
                else
                {
                    await ShowErrorAsync("Failed to add word");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error adding word", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task CancelAsync()
        {
            await _navigationService.NavigateToWordListAsync();
        }
    }
} 