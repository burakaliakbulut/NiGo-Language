using System;
using System.Threading.Tasks;

namespace WordLearningWpfApp.Services
{
    public interface IService : IDisposable
    {
        Task InitializeAsync();
        Task CleanupAsync();
    }

    public class ServiceException : Exception
    {
        public ServiceException(string message) : base(message) { }
        public ServiceException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ServiceInitializationException : ServiceException
    {
        public ServiceInitializationException(string message) : base(message) { }
        public ServiceInitializationException(string message, Exception innerException) : base(message, innerException) { }
    }
} 