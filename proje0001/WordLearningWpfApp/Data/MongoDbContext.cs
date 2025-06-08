using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Threading.Tasks;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Data
{
    public class MongoDbContext : IDbContext
    {
        private readonly IMongoDatabase _database;

        // Collection names
        public const string UsersCollection = "users";
        public const string WordsCollection = "words";
        public const string UserWordProgressCollection = "userWordProgress";
        public const string StatisticsCollection = "Statistics";
        public const string WordDifficultyLevelsCollection = "wordDifficultyLevels";
        public const string NotificationsCollection = "notifications";
        public const string ExamsCollection = "exams";
        public const string QuestionsCollection = "questions";
        public const string ExamResultsCollection = "examResults";
        public const string DailyProgressCollection = "dailyProgress";
        public const string ExamStatisticsCollection = "examStatistics";

        public IMongoCollection<User> Users => _database.GetCollection<User>(UsersCollection);
        public IMongoCollection<Word> Words => _database.GetCollection<Word>(WordsCollection);
        public IMongoCollection<WordProgress> WordProgress => _database.GetCollection<WordProgress>(UserWordProgressCollection);
        public IMongoCollection<Statistics> Statistics => _database.GetCollection<Statistics>(StatisticsCollection);
        public IMongoCollection<Exam> Exams => _database.GetCollection<Exam>(ExamsCollection);
        public IMongoCollection<Question> Questions => _database.GetCollection<Question>(QuestionsCollection);
        public IMongoCollection<ExamResult> ExamResults => _database.GetCollection<ExamResult>(ExamResultsCollection);
        public IMongoCollection<DailyProgress> DailyProgress => _database.GetCollection<DailyProgress>(DailyProgressCollection);
        public IMongoCollection<ExamStatistics> ExamStatistics => _database.GetCollection<ExamStatistics>(ExamStatisticsCollection);
        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>(NotificationsCollection);

        public MongoDbContext(IConfiguration configuration)
        {
            var settings = configuration.GetSection("MongoDbSettings");
            var client = new MongoClient(settings["ConnectionString"]);
            _database = client.GetDatabase(settings["DatabaseName"]);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Collection name cannot be empty", nameof(name));

            return _database.GetCollection<T>(name);
        }

        public async Task InitializeAsync()
        {
            await InitializeDatabaseAsync();
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                // Create collections
                await CreateCollectionIfNotExistsAsync<User>(UsersCollection);
                await CreateCollectionIfNotExistsAsync<Word>(WordsCollection);
                await CreateCollectionIfNotExistsAsync<UserWordProgress>(UserWordProgressCollection);
                await CreateCollectionIfNotExistsAsync<Statistics>(StatisticsCollection);
                await CreateCollectionIfNotExistsAsync<WordDifficultyLevel>(WordDifficultyLevelsCollection);
                await CreateCollectionIfNotExistsAsync<Notification>(NotificationsCollection);
                await CreateCollectionIfNotExistsAsync<Exam>(ExamsCollection);
                await CreateCollectionIfNotExistsAsync<Question>(QuestionsCollection);
                await CreateCollectionIfNotExistsAsync<ExamResult>(ExamResultsCollection);
                await CreateCollectionIfNotExistsAsync<DailyProgress>(DailyProgressCollection);
                await CreateCollectionIfNotExistsAsync<ExamStatistics>(ExamStatisticsCollection);

                // Create indexes
                await CreateIndexesAsync();

                // Add default difficulty levels
                await InitializeDefaultDifficultyLevelsAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Database initialization failed: {ex.Message}", ex);
            }
        }

        private async Task CreateCollectionIfNotExistsAsync<T>(string name)
        {
            try
            {
                var collections = await _database.ListCollectionNamesAsync();
                var collectionNames = await _database.ListCollectionNames().ToListAsync();
                bool exists = collectionNames.Contains(name);

                if (!exists)
                {
                    await _database.CreateCollectionAsync(name);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create collection: {name}. Error: {ex.Message}", ex);
            }
        }

        private async Task InitializeDefaultDifficultyLevelsAsync()
        {
            var difficultyLevels = GetCollection<WordDifficultyLevel>(WordDifficultyLevelsCollection);
            if (await difficultyLevels.CountDocumentsAsync(FilterDefinition<WordDifficultyLevel>.Empty) == 0)
            {
                await difficultyLevels.InsertManyAsync(WordDifficultyLevel.GetDefaultLevels());
            }
        }

        private async Task CreateIndexesAsync()
        {
            try
            {
                // Users collection indexes
                var userIndexKeys = Builders<User>.IndexKeys.Ascending(u => u.Email);
                var userIndexOptions = new CreateIndexOptions { Unique = true };
                var userIndexModel = new CreateIndexModel<User>(userIndexKeys, userIndexOptions);
                await Users.Indexes.CreateOneAsync(userIndexModel);

                // Words collection indexes
                var wordIndexKeys = Builders<Word>.IndexKeys.Ascending(w => w.Text);
                var wordIndexOptions = new CreateIndexOptions { Unique = true };
                var wordIndexModel = new CreateIndexModel<Word>(wordIndexKeys, wordIndexOptions);
                await Words.Indexes.CreateOneAsync(wordIndexModel);

                // UserWordProgress collection indexes
                var progressCollection = GetCollection<UserWordProgress>(UserWordProgressCollection);
                var progressIndexes = Builders<UserWordProgress>.IndexKeys;
                await progressCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserWordProgress>(
                    progressIndexes.Ascending(p => p.UserId).Ascending(p => p.WordId),
                    new CreateIndexOptions { Unique = true }
                ));
                await progressCollection.Indexes.CreateOneAsync(new CreateIndexModel<UserWordProgress>(
                    progressIndexes.Ascending(p => p.NextReview)
                ));

                // Statistics collection indexes
                var statsCollection = GetCollection<Statistics>(StatisticsCollection);
                var statsIndexes = Builders<Statistics>.IndexKeys;
                await statsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Statistics>(
                    statsIndexes.Ascending(s => s.UserId).Ascending(s => s.Date),
                    new CreateIndexOptions { Unique = true }
                ));

                // Notifications collection indexes
                var notificationsCollection = GetCollection<Notification>(NotificationsCollection);
                var notificationIndexes = Builders<Notification>.IndexKeys;
                await notificationsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Notification>(
                    notificationIndexes.Ascending(n => n.UserId).Ascending(n => n.CreatedAt)
                ));
                await notificationsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Notification>(
                    notificationIndexes.Ascending(n => n.IsRead)
                ));

                // Exams collection indexes
                var examsCollection = GetCollection<Exam>(ExamsCollection);
                var examsIndexes = Builders<Exam>.IndexKeys;
                await examsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Exam>(
                    examsIndexes.Ascending(e => e.UserId).Ascending(e => e.CreatedAt)
                ));

                // Questions collection indexes
                var questionsCollection = GetCollection<Question>(QuestionsCollection);
                var questionsIndexes = Builders<Question>.IndexKeys;
                await questionsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Question>(
                    questionsIndexes.Ascending(q => q.ExamId)
                ));

                // ExamResults collection indexes
                var examResultsCollection = GetCollection<ExamResult>(ExamResultsCollection);
                var examResultsIndexes = Builders<ExamResult>.IndexKeys;
                await examResultsCollection.Indexes.CreateOneAsync(new CreateIndexModel<ExamResult>(
                    examResultsIndexes.Ascending(r => r.UserId).Ascending(r => r.ExamDate)
                ));
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create indexes: {ex.Message}", ex);
            }
        }
    }
} 