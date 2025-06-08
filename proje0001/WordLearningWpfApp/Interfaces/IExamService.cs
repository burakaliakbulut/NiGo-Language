using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Interfaces;

public interface IExamService
{
    Task<Exam> CreateExamAsync(string userId, int wordCount = 20);
    Task<ExamResult> SubmitExamAsync(string examId, List<QuestionAnswer> answers);
    Task<Exam?> GetExamAsync(string examId);
    Task<List<Exam>> GetUserExamsAsync(string userId, int skip = 0, int limit = 20);
    Task<List<Exam>> GetRecentExamsAsync(int limit = 10);
    Task<ExamStatistics> GetUserStatisticsAsync(string userId);
    Task<bool> DeleteExamAsync(string examId);
}

public class QuestionAnswer
{
    public string QuestionId { get; set; } = string.Empty;
    public string SelectedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class Exam
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<Question> Questions { get; set; } = new();
    public double Score { get; set; }
    public bool IsCompleted { get; set; }
}

public class Question
{
    public string Id { get; set; } = string.Empty;
    public string WordId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
}

public class ExamResult
{
    public string ExamId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public double Score { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public List<QuestionAnswer> Answers { get; set; } = new();
}

public class ExamStatistics
{
    public string UserId { get; set; } = string.Empty;
    public int TotalExams { get; set; }
    public double AverageScore { get; set; }
    public int HighestScore { get; set; }
    public int LowestScore { get; set; }
    public Dictionary<string, int> CategoryPerformance { get; set; } = new();
    public Dictionary<int, int> DifficultyPerformance { get; set; } = new();
} 