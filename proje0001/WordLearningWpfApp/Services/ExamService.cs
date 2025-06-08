using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;
using MongoDB.Bson;

namespace WordLearningWpfApp.Services
{
    public class ExamService : IExamService
    {
        private readonly IMongoCollection<Exam> _exams;
        private readonly IMongoCollection<Question> _questions;
        private readonly IMongoCollection<ExamResult> _examResults;
        private readonly IWordService _wordService;
        private readonly IStatisticsService _statisticsService;
        private readonly IMongoCollection<Word> _words;
        private readonly IMongoCollection<WordProgress> _wordProgress;
        private readonly IAuthService _authService;
        private readonly IDbContext _dbContext;

        public ExamService(
            IDbContext dbContext,
            IWordService wordService,
            IStatisticsService statisticsService,
            IAuthService authService)
        {
            _exams = dbContext.Exams;
            _questions = dbContext.Questions;
            _examResults = dbContext.ExamResults;
            _wordService = wordService;
            _statisticsService = statisticsService;
            _words = dbContext.Words;
            _wordProgress = dbContext.WordProgress;
            _authService = authService;
            _dbContext = dbContext;
        }

        public async Task<Exam> CreateExamAsync(string userId, ExamType type)
        {
            var exam = new Exam
            {
                UserId = userId,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                Questions = new List<Question>()
            };

            await _exams.InsertOneAsync(exam);
            return exam;
        }

        public async Task<List<Exam>> GetUserExamsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _exams.Find(e => e.UserId == userId)
                .SortByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<ExamResult> SubmitExamAsync(string userId, string examId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrEmpty(examId))
                throw new ArgumentException("Exam ID cannot be null or empty", nameof(examId));

            var exam = await GetExamAsync(examId);
            if (exam == null)
                throw new ArgumentException("Exam not found", nameof(examId));

            if (exam.Status != ExamStatus.InProgress)
                throw new InvalidOperationException("Exam is not in progress");

            var questions = await _dbContext.Questions
                .Find(q => q.ExamId == examId)
                .ToListAsync();

            var correctAnswers = questions.Count(q => q.IsCorrect);
            var totalQuestions = questions.Count;

            var result = new ExamResult
            {
                UserId = userId,
                ExamId = examId,
                TotalQuestions = totalQuestions,
                CorrectAnswers = correctAnswers,
                IncorrectAnswers = totalQuestions - correctAnswers,
                Score = totalQuestions > 0 ? (double)correctAnswers / totalQuestions * 100 : 0,
                StartedAt = exam.StartedAt,
                CompletedAt = DateTime.UtcNow,
                Status = ExamStatus.Completed,
                Difficulty = exam.Difficulty,
                Category = exam.Category
            };

            await _dbContext.ExamResults.InsertOneAsync(result);

            exam.Status = ExamStatus.Completed;
            await _dbContext.Exams.ReplaceOneAsync(e => e.Id == examId, exam);

            return result;
        }

        public async Task<List<Exam>> GetExamsByTypeAsync(string userId, ExamType type)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _exams.Find(e => e.UserId == userId && e.Type == type)
                .SortByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<Exam?> GetLatestExamAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _exams.Find(e => e.UserId == userId)
                .SortByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Exam?> GetNextExamAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            var latestExam = await GetLatestExamAsync(userId);
            if (latestExam == null || latestExam.IsCompleted)
            {
                return await GenerateExamAsync(userId, ExamType.Daily);
            }
            return latestExam;
        }

        public async Task<Exam> GenerateExamAsync(string userId, ExamType type)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new UnauthorizedAccessException("User is not authenticated");

            var words = await _wordService.GetWordsByUserIdAsync(userId);
            if (!words.Any())
                throw new InvalidOperationException("No words available for exam");

            var random = new Random();
            var selectedWords = words.OrderBy(x => random.Next()).Take(10).ToList();
            var questions = new List<Question>();

            foreach (var word in selectedWords)
            {
                var question = new Question
                {
                    WordId = word.Id,
                    English = word.English,
                    CorrectAnswer = word.Turkish,
                    Options = GenerateOptions(word.Turkish, words.Select(w => w.Turkish).ToList(), random)
                };
                questions.Add(question);
            }

            var exam = new Exam
            {
                UserId = userId,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                Questions = questions
            };

            await _exams.InsertOneAsync(exam);
            return exam;
        }

        public async Task<bool> CheckAnswerAsync(string userId, string questionId, string answer)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrEmpty(questionId))
                throw new ArgumentException("Question ID cannot be null or empty", nameof(questionId));

            if (string.IsNullOrEmpty(answer))
                throw new ArgumentException("Answer cannot be null or empty", nameof(answer));

            var question = await _dbContext.Questions
                .Find(q => q.Id == questionId)
                .FirstOrDefaultAsync();

            if (question == null)
                throw new ArgumentException("Question not found", nameof(questionId));

            var isCorrect = answer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            question.IsCorrect = isCorrect;
            question.UserAnswer = answer;
            question.AnsweredAt = DateTime.UtcNow;

            await _dbContext.Questions.ReplaceOneAsync(q => q.Id == questionId, question);

            // Update word progress
            var progress = await _dbContext.WordProgress
                .Find(p => p.UserId == userId && p.WordId == question.WordId)
                .FirstOrDefaultAsync();

            if (progress == null)
            {
                progress = new WordProgress
                {
                    UserId = userId,
                    WordId = question.WordId,
                    Status = WordStatus.Learning,
                    CorrectAnswers = isCorrect ? 1 : 0,
                    IncorrectAnswers = isCorrect ? 0 : 1,
                    LastAttempted = DateTime.UtcNow,
                    LastCorrect = isCorrect ? DateTime.UtcNow : null,
                    NextReview = DateTime.UtcNow.AddDays(1),
                    Difficulty = question.Difficulty,
                    Category = question.Category
                };
                await _dbContext.WordProgress.InsertOneAsync(progress);
            }
            else
            {
                if (isCorrect)
                    progress.CorrectAnswers++;
                else
                    progress.IncorrectAnswers++;

                progress.LastAttempted = DateTime.UtcNow;
                if (isCorrect)
                    progress.LastCorrect = DateTime.UtcNow;

                progress.Status = CalculateWordStatus(progress);
                progress.NextReview = CalculateNextReview(progress);

                await _dbContext.WordProgress.ReplaceOneAsync(p => p.Id == progress.Id, progress);
            }

            return isCorrect;
        }

        public async Task<ExamResult> SubmitExamAsync(string userId, List<Question> questions)
        {
            var exam = await _exams.Find(e => e.UserId == userId && !e.IsCompleted)
                .SortByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();

            if (exam == null)
                throw new ArgumentException($"No active exam found for user: {userId}");

            exam.Questions = questions;
            exam.CompletedAt = DateTime.UtcNow;

            await _exams.ReplaceOneAsync(e => e.Id == exam.Id, exam);

            var result = new ExamResult
            {
                ExamId = exam.Id,
                UserId = userId,
                CompletedAt = DateTime.UtcNow,
                TotalQuestions = questions.Count,
                CorrectAnswers = questions.Count(q => q.IsCorrect),
                IncorrectAnswers = questions.Count(q => !q.IsCorrect),
                Score = (double)questions.Count(q => q.IsCorrect) / questions.Count * 100
            };

            await _examResults.InsertOneAsync(result);
            return result;
        }

        public async Task<List<Question>> GenerateExamAsync(string userId)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new UnauthorizedAccessException("User is not authenticated");

            var words = await _wordService.GetWordsByUserIdAsync(userId);
            if (!words.Any())
                throw new InvalidOperationException("No words available for exam");

            var random = new Random();
            var selectedWords = words.OrderBy(x => random.Next()).Take(10).ToList();
            var questions = new List<Question>();

            foreach (var word in selectedWords)
            {
                var question = new Question
                {
                    WordId = word.Id,
                    English = word.English,
                    CorrectAnswer = word.Turkish,
                    Options = GenerateOptions(word.Turkish, words.Select(w => w.Turkish).ToList(), random)
                };
                questions.Add(question);
            }

            return questions;
        }

        public async Task<bool> CheckAnswerAsync(string userId, string questionId)
        {
            var examResult = await _examResults.Find(e => e.UserId == userId && e.Status == ExamStatus.InProgress)
                .FirstOrDefaultAsync();

            if (examResult == null)
            {
                throw new Exception("No active exam found");
            }

            var question = examResult.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null)
            {
                throw new Exception("Question not found");
            }

            question.IsAnswered = true;
            question.IsCorrect = question.UserAnswer == question.CorrectAnswer;

            var update = Builders<ExamResult>.Update
                .Set(e => e.Questions, examResult.Questions);

            await _examResults.UpdateOneAsync(e => e.Id == examResult.Id, update);
            return question.IsCorrect;
        }

        public async Task<ExamResult> CompleteExamAsync(string userId, int correctAnswers, int totalQuestions)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (correctAnswers < 0)
                throw new ArgumentException("Correct answers cannot be negative", nameof(correctAnswers));
            if (totalQuestions <= 0)
                throw new ArgumentException("Total questions must be greater than zero", nameof(totalQuestions));
            if (correctAnswers > totalQuestions)
                throw new ArgumentException("Correct answers cannot be greater than total questions");

            var examResult = await _examResults.Find(e => e.UserId == userId && e.Status == ExamStatus.InProgress)
                .FirstOrDefaultAsync();

            if (examResult == null)
                throw new InvalidOperationException("No active exam found");

            examResult.EndTime = DateTime.UtcNow;
            examResult.Status = ExamStatus.Completed;
            examResult.Score = (double)correctAnswers / totalQuestions * 100;
            examResult.CorrectAnswers = correctAnswers;
            examResult.TotalQuestions = totalQuestions;
            examResult.IncorrectAnswers = totalQuestions - correctAnswers;

            var update = Builders<ExamResult>.Update
                .Set(e => e.EndTime, examResult.EndTime)
                .Set(e => e.Status, examResult.Status)
                .Set(e => e.Score, examResult.Score)
                .Set(e => e.CorrectAnswers, examResult.CorrectAnswers)
                .Set(e => e.TotalQuestions, examResult.TotalQuestions)
                .Set(e => e.IncorrectAnswers, examResult.IncorrectAnswers);

            await _examResults.UpdateOneAsync(e => e.Id == examResult.Id, update);

            // Update statistics
            var statistics = await _statisticsService.GetUserStatisticsAsync(userId);
            statistics.TotalExams++;
            statistics.TotalCorrectAnswers += correctAnswers;
            statistics.TotalQuestions += totalQuestions;
            statistics.AverageScore = (statistics.TotalCorrectAnswers * 100.0) / statistics.TotalQuestions;
            await _statisticsService.UpdateStatisticsAsync(userId, statistics);

            return examResult;
        }

        public async Task<List<ExamResult>> GetUserExamHistoryAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _examResults.Find(e => e.UserId == userId && e.Status == ExamStatus.Completed)
                .SortByDescending(e => e.EndTime)
                .ToListAsync();
        }

        public async Task<ExamResult?> GetLastExamResultAsync(string userId)
        {
            return await _examResults.Find(r => r.UserId == userId)
                .SortByDescending(r => r.CompletedAt)
                .FirstOrDefaultAsync();
        }

        private List<string> GenerateOptions(string correctAnswer, List<string> allAnswers, Random random)
        {
            var options = new List<string> { correctAnswer };
            var otherAnswers = allAnswers.Where(a => a != correctAnswer).ToList();
            
            while (options.Count < 4 && otherAnswers.Any())
            {
                var index = random.Next(otherAnswers.Count);
                options.Add(otherAnswers[index]);
                otherAnswers.RemoveAt(index);
            }

            return options.OrderBy(x => random.Next()).ToList();
        }

        private WordStatus CalculateWordStatus(WordProgress progress)
        {
            var totalAttempts = progress.CorrectAnswers + progress.IncorrectAnswers;
            if (totalAttempts == 0) return WordStatus.New;

            var successRate = (double)progress.CorrectAnswers / totalAttempts;
            if (successRate >= 0.8) return WordStatus.Learned;
            if (successRate >= 0.6) return WordStatus.Learning;
            return WordStatus.Difficult;
        }

        private DateTime CalculateNextReview(WordProgress progress)
        {
            var baseInterval = TimeSpan.FromDays(1);
            var multiplier = progress.Status switch
            {
                WordStatus.Learned => 2.0,
                WordStatus.Learning => 1.0,
                WordStatus.Difficult => 0.5,
                _ => 1.0
            };

            return DateTime.UtcNow.Add(baseInterval * multiplier);
        }

        public async Task<Exam> CreateExamAsync(string userId, ExamType type, int questionCount)
        {
            // Implementation for creating an exam with the specified number of questions
            return await GenerateExamAsync(userId, type, questionCount);
        }

        public async Task<Exam> GenerateExamAsync(string userId, ExamType type, int questionCount)
        {
            // Implementation for generating an exam
            // This should select random words and create questions accordingly
            // For now, just call the existing logic or return a placeholder
            // You can expand this as needed
            var exam = new Exam
            {
                UserId = userId,
                Type = type,
                Status = ExamStatus.NotStarted,
                StartedAt = DateTime.UtcNow,
                TotalQuestions = questionCount
            };
            await _dbContext.Exams.InsertOneAsync(exam);
            return exam;
        }

        public async Task<Exam?> GetExamAsync(string examId)
        {
            return await _dbContext.Exams.Find(e => e.Id == examId).FirstOrDefaultAsync();
        }

        public async Task<ExamResult?> GetExamResultAsync(string examId)
        {
            return await _dbContext.ExamResults.Find(r => r.ExamId == examId).FirstOrDefaultAsync();
        }
    }
} 