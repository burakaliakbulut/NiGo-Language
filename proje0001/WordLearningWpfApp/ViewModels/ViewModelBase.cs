using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using WordLearningWpfApp.Interfaces;
using WordLearningWpfApp.Services;

namespace WordLearningWpfApp.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        protected readonly INavigationService _navigationService;
        protected readonly INotificationService _notificationService;
        private bool _isBusy;
        private string? _errorMessage;
        private bool _hasError;
        private bool _disposedValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected ViewModelBase(
            INavigationService navigationService,
            INotificationService notificationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public virtual bool IsBusy
        {
            get => _isBusy;
            protected set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged();
                }
            }
        }

        public virtual string? ErrorMessage
        {
            get => _errorMessage;
            protected set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public virtual bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(ref T field, T value, string[] dependentProperties, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            foreach (var dependentProperty in dependentProperties)
            {
                OnPropertyChanged(dependentProperty);
            }
            return true;
        }

        protected async Task ExecuteAsync(Func<Task> operation, string? errorMessage = null)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                await operation();
            }
            catch (Exception ex)
            {
                ErrorMessage = errorMessage ?? ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, string? errorMessage = null)
        {
            try
            {
                IsBusy = true;
                ErrorMessage = null;
                return await operation();
            }
            catch (Exception ex)
            {
                ErrorMessage = errorMessage ?? ex.Message;
                return default;
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual async Task ShowErrorAsync(string message, Exception? ex = null)
        {
            var errorMessage = ex != null ? $"{message}: {ex.Message}" : message;
            await _notificationService.ShowErrorAsync(errorMessage, ex);
        }

        protected virtual async Task ShowSuccessAsync(string message)
        {
            await _notificationService.ShowSuccessAsync(message);
        }

        protected virtual async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            return await _notificationService.ShowConfirmationAsync(title, message);
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task CleanupAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
} 