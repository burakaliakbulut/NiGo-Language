using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Proje0001.Services
{
    public class UserService
    {
        private readonly IMongoDatabase _context;

        public UserService(IMongoDatabase context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, int>> GetUserRoleDistributionAsync()
        {
            try
            {
                var collection = _context.GetCollection<User>("Users");
                var users = await collection.Find(_ => true).ToListAsync();
                
                var roleDistribution = new Dictionary<string, int>();
                foreach (var user in users)
                {
                    var roleName = user.Role.ToString();
                    if (!roleDistribution.ContainsKey(roleName))
                    {
                        roleDistribution[roleName] = 0;
                    }
                    roleDistribution[roleName]++;
                }
                
                return roleDistribution;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get user role distribution", ex);
            }
        }

        public async Task<bool> IsUserInRoleAsync(string userId, UserRole role)
        {
            try
            {
                var collection = _context.GetCollection<User>("Users");
                var user = await collection.Find(u => u.Id == userId).FirstOrDefaultAsync();
                return user?.Role == role;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to check user role", ex);
            }
        }
    }
} 