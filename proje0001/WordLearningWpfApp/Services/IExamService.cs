using System.Collections.Generic;
using System.Threading.Tasks;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Services
{
    public interface IExamService
    {
        Task<Exam> CreateExamAsync(string userId, ExamType type, int questionCount);
        Task<Exam?> GetExamAsync(string examId);
        Task<List<Exam>> GetUserExamsAsync(string userId);
        Task<ExamResult?> GetExamResultAsync(string examId);
        Task<List<Exam>> GetExamsByTypeAsync(string userId, ExamType type);
        Task<Exam?> GetLatestExamAsync(string userId);
        Task<Exam?> GetNextExamAsync(string userId);
        Task<bool> CheckAnswerAsync(string userId, string questionId, string answer);
        Task<ExamResult> SubmitExamAsync(string userId, string examId);
        Task<ExamResult> CompleteExamAsync(string userId, int correctAnswers, int totalQuestions);
        Task<List<ExamResult>> GetUserExamHistoryAsync(string userId);
        Task<ExamResult?> GetLastExamResultAsync(string userId);
        Task<Exam> GenerateExamAsync(string userId, ExamType type, int questionCount);
    }
} 