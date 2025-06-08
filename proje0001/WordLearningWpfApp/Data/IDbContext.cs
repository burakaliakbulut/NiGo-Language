using MongoDB.Driver;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Data
{
    public interface IDbContext
    {
        IMongoCollection<User> Users { get; }
        IMongoCollection<Word> Words { get; }
        IMongoCollection<WordProgress> WordProgress { get; }
        IMongoCollection<Statistics> Statistics { get; }
        IMongoCollection<Exam> Exams { get; }
        IMongoCollection<Question> Questions { get; }
        IMongoCollection<ExamResult> ExamResults { get; }
        IMongoCollection<DailyProgress> DailyProgress { get; }
        IMongoCollection<ExamStatistics> ExamStatistics { get; }
        IMongoCollection<Notification> Notifications { get; }
        IMongoCollection<T> GetCollection<T>(string name);
    }
} 