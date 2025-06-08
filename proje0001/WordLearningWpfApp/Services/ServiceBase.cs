using System;
using System.Threading.Tasks;

namespace WordLearningWpfApp.Services
{
    public abstract class ServiceBase : IService
    {
        private bool _isDisposed;
        private bool _isInitialized;

        public bool IsInitialized => _isInitialized;

        public virtual async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                await ValidateConnectionAsync();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new ServiceInitializationException("Failed to initialize service", ex);
            }
        }

        public virtual Task CleanupAsync()
        {
            return Task.CompletedTask;
        }

        protected abstract Task ValidateConnectionAsync();

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
} 