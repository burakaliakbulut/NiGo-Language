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
    public class WordListViewModel : ViewModelBase
    {
        private readonly IWordService _wordService;
        private readonly IAuthService _authService;
        protected new readonly INavigationService _navigationService;
        protected new readonly INotificationService _notificationService;
        private ObservableCollection<Word> _words = new();
        private Word? _selectedWord;
        private string _searchText = string.Empty;
        private bool _isBusy;

        public WordListViewModel(
            IWordService wordService,
            IAuthService authService,
            INavigationService navigationService,
            INotificationService notificationService)
            : base(navigationService, notificationService)
        {
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            AddWordCommand = new RelayCommand(async _ => await AddWordAsync());
            EditWordCommand = new RelayCommand(async _ => await EditWordAsync(), _ => SelectedWord != null);
            DeleteWordCommand = new RelayCommand(async _ => await DeleteWordAsync(), _ => SelectedWord != null);
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            BackCommand = new RelayCommand(async _ => await BackAsync());

            LoadWordsAsync().ConfigureAwait(false);
        }

        public ObservableCollection<Word> Words
        {
            get => _words;
            private set
            {
                _words = value;
                OnPropertyChanged(nameof(Words));
            }
        }

        public Word? SelectedWord
        {
            get => _selectedWord;
            set
            {
                _selectedWord = value;
                OnPropertyChanged(nameof(SelectedWord));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public ICommand AddWordCommand { get; }
        public ICommand EditWordCommand { get; }
        public ICommand DeleteWordCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand BackCommand { get; }

        private async Task LoadWordsAsync()
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

                var words = await _wordService.GetAllWordsAsync(currentUser.Id);
                Words = new ObservableCollection<Word>(words);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error loading words", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task AddWordAsync()
        {
            await _navigationService.NavigateToAddWordAsync();
        }

        private async Task EditWordAsync()
        {
            if (SelectedWord == null) return;

            await _navigationService.NavigateToEditWordAsync(SelectedWord.Id);
        }

        private async Task DeleteWordAsync()
        {
            if (SelectedWord == null) return;

            try
            {
                var result = await _wordService.DeleteWordAsync(SelectedWord.Id);
                if (result)
                {
                    Words.Remove(SelectedWord);
                    SelectedWord = null;
                    await ShowSuccessAsync("Success", "Word deleted successfully");
                }
                else
                {
                    await ShowErrorAsync("Failed to delete word");
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error deleting word", ex);
            }
        }

        private async Task SearchAsync()
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

                var words = await _wordService.SearchWordsAsync(currentUser.Id, SearchText);
                Words = new ObservableCollection<Word>(words);
            }
            catch (Exception ex)
            {
                await ShowErrorAsync("Error searching words", ex);
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