using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Bson;
using WordLearningWpfApp.Data;
using WordLearningWpfApp.Models;

namespace WordLearningWpfApp.Repository
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbContext context)
        {
            _users = context.Users;
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task UpdateDailyWordLimitAsync(string userId, int newLimit)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.DailyWordLimit, newLimit);
            await _users.UpdateOneAsync(filter, update);
        }
    }
} 