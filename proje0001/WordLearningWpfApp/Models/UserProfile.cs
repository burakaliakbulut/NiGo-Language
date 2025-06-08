using System;

namespace WordLearningWpfApp.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public UserSettings Settings { get; set; } = new UserSettings();
    }
} 