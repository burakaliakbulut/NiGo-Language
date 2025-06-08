using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WordLearningWpfApp.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action? _execute;
        private readonly Action<object?>? _executeWithParameter;
        private readonly Func<Task>? _executeAsync;
        private readonly Func<object?, Task>? _executeAsyncWithParameter;
        private readonly Predicate<object?>? _canExecute;
        private bool _isExecuting;

        public RelayCommand(Action execute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public RelayCommand(Func<Task> executeAsync)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
        }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _executeWithParameter = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Func<object?, Task> executeAsync, Predicate<object?>? canExecute = null)
        {
            _executeAsyncWithParameter = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                CommandManager.InvalidateRequerySuggested();

                if (_execute != null)
                {
                    _execute();
                }
                else if (_executeWithParameter != null)
                {
                    _executeWithParameter(parameter);
                }
                else if (_executeAsync != null)
                {
                    await _executeAsync();
                }
                else if (_executeAsyncWithParameter != null)
                {
                    await _executeAsyncWithParameter(parameter);
                }
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
} 