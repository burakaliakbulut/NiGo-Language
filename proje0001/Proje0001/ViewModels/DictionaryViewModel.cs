using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Proje0001.Models;
using Proje0001.Services;

namespace Proje0001.ViewModels
{
    public class DictionaryViewModel : ViewModelBase, IInitializable
    {
        private readonly IWordService _wordService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUser _currentUser;

        private ObservableCollection<Word> _words;
        private ObservableCollection<string> _categories;
        private string _selectedCategory;
        private string _searchText;
        private bool _isLoading;
        private string _errorMessage;

        public ObservableCollection<Word> Words
        {
            get => _words;
            set
            {
                _words = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                _ = LoadWordsAsync();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                _ = FilterWordsAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddWordCommand { get; }
        public ICommand EditWordCommand { get; }
        public ICommand DeleteWordCommand { get; }

        public DictionaryViewModel(
            IWordService wordService,
            INavigationService navigationService,
            INotificationService notificationService,
            ICurrentUser currentUser)
        {
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            Words = new ObservableCollection<Word>();
            Categories = new ObservableCollection<string>();

            RefreshCommand = new RelayCommand(async _ => await LoadWordsAsync());
            AddWordCommand = new RelayCommand(async _ => await NavigateToAddWordAsync());
            EditWordCommand = new RelayCommand<Word>(async word => await NavigateToEditWordAsync(word));
            DeleteWordCommand = new RelayCommand<Word>(async word => await DeleteWordAsync(word));
        }

        public async Task InitializeAsync(object parameter = null)
        {
            try
            {
                await LoadCategoriesAsync();
                await LoadWordsAsync();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Failed to initialize dictionary view", ex);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                IsLoading = true;
                var categories = await _wordService.GetCategoriesAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Failed to load categories", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadWordsAsync()
        {
            try
            {
                IsLoading = true;
                var words = await _wordService.GetWordsAsync(SelectedCategory);
                Words.Clear();
                foreach (var word in words)
                {
                    Words.Add(word);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Failed to load words", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FilterWordsAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadWordsAsync();
                    return;
                }

                var filteredWords = Words.Where(w =>
                    w.Text.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    w.Translation.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    w.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

                Words.Clear();
                foreach (var word in filteredWords)
                {
                    Words.Add(word);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Failed to filter words", ex);
            }
        }

        private async Task NavigateToAddWordAsync()
        {
            try
            {
                await _navigationService.NavigateToAsync("AddWord");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Navigation failed", ex);
            }
        }

        private async Task NavigateToEditWordAsync(Word word)
        {
            try
            {
                if (word == null)
                    return;

                await _navigationService.NavigateToAsync("EditWord", word);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Navigation failed", ex);
            }
        }

        private async Task DeleteWordAsync(Word word)
        {
            try
            {
                if (word == null)
                    return;

                var confirm = _navigationService.ShowConfirmation(
                    $"Are you sure you want to delete the word '{word.Text}'?",
                    "Delete Word");

                if (!confirm)
                    return;

                await _wordService.DeleteWordAsync(word.Id);
                Words.Remove(word);
                _notificationService.ShowSuccess("Word deleted successfully");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Failed to delete word", ex);
            }
        }
    }
} 