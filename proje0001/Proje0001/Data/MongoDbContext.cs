using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Proje0001.Data
{
    public class MongoDbContext
    {
        private readonly IMongoClient _client;
        private readonly string _databaseName;
        private IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            _client = new MongoClient(connectionString);
            _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _client.ConnectAsync();
                _database = _client.GetDatabase(_databaseName);

                // Create indexes for better query performance
                await CreateIndexesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize database", ex);
            }
        }

        private async Task CreateIndexesAsync()
        {
            // Users collection indexes
            var usersCollection = _database.GetCollection<BsonDocument>("Users");
            var usersIndexes = new List<CreateIndexModel<BsonDocument>>
            {
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("Email"), new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("Role"))
            };
            await usersCollection.Indexes.CreateManyAsync(usersIndexes);

            // Words collection indexes
            var wordsCollection = _database.GetCollection<BsonDocument>("Words");
            var wordsIndexes = new List<CreateIndexModel<BsonDocument>>
            {
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("Category")),
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("DifficultyLevel")),
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Text("Text").Text("Translation"))
            };
            await wordsCollection.Indexes.CreateManyAsync(wordsIndexes);

            // UserWordProgress collection indexes
            var progressCollection = _database.GetCollection<BsonDocument>("UserWordProgress");
            var progressIndexes = new List<CreateIndexModel<BsonDocument>>
            {
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("UserId")),
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("WordId")),
                new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("Status"))
            };
            await progressCollection.Indexes.CreateManyAsync(progressIndexes);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return _database.GetCollection<T>(name);
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task DropDatabaseAsync()
        {
            await _client.DropDatabaseAsync(_databaseName);
        }

        public async Task CreateCollectionIfNotExistsAsync(string collectionName)
        {
            var collections = await _database.ListCollectionNames().ToListAsync();
            if (!collections.Contains(collectionName))
            {
                await _database.CreateCollectionAsync(collectionName);
            }
        }
    }
} 