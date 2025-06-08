using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordLearningWpfApp.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        [BsonElement("dailyWordLimit")]
        public int DailyWordLimit { get; set; } = 10;

        [BsonElement("defaultDifficultyLevel")]
        public int DefaultDifficultyLevel { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        [BsonElement("lastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("enableNotifications")]
        public bool EnableNotifications { get; set; }

        [BsonElement("preferences")]
        public UserPreferences Preferences { get; set; }

        [BsonElement("statistics")]
        public UserStatistics Statistics { get; set; }

        [BsonElement("role")]
        public UserRole Role { get; set; }

        [BsonElement("isDarkMode")]
        public bool IsDarkMode { get; set; }

        [BsonElement("isNotificationsEnabled")]
        public bool IsNotificationsEnabled { get; set; }

        [BsonElement("language")]
        public string Language { get; set; } = "en-US";

        [BsonElement("profile")]
        public UserProfile Profile { get; set; } = new UserProfile();

        [BsonElement("settings")]
        public UserSettings Settings { get; set; } = new UserSettings();

        [BsonElement("passwordResetToken")]
        public string PasswordResetToken { get; set; } = string.Empty;

        [BsonElement("passwordResetTokenExpiry")]
        public DateTime? PasswordResetTokenExpiry { get; set; }

        [BsonElement("isEmailVerified")]
        public bool IsEmailVerified { get; set; }

        [BsonElement("emailVerificationToken")]
        public string EmailVerificationToken { get; set; } = string.Empty;

        [BsonElement("currentStreak")]
        public int CurrentStreak { get; set; }

        [BsonElement("lastLogin")]
        public DateTime? LastLogin { get; set; }

        [BsonElement("isAdmin")]
        public bool IsAdmin { get; set; }

        [NotMapped]
        public string Password
        {
            get => PasswordHash;
            set => PasswordHash = value;
        }

        public User()
        {
            IsActive = true;
            DailyWordLimit = 10;
            DefaultDifficultyLevel = 1;
            CreatedAt = DateTime.UtcNow;
            LastLoginAt = DateTime.UtcNow;
            LastLoginDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Username = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            EnableNotifications = true;
            Preferences = new UserPreferences();
            Statistics = new UserStatistics();
            Role = UserRole.User;
            CurrentStreak = 0;
        }

        public User(string username, string email, string passwordHash, string firstName, string lastName)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            FirstName = firstName;
            LastName = lastName;
            DailyWordLimit = 10;
            EnableNotifications = true;
            DefaultDifficultyLevel = 1;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            LastLoginDate = DateTime.UtcNow;
            IsActive = true;
            Preferences = new UserPreferences();
            Statistics = new UserStatistics();
            Role = UserRole.User;
            CurrentStreak = 0;
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}".Trim();
        }

        public void UpdateLastLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            LastLoginDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePreferences(UserPreferences newPreferences)
        {
            Preferences = newPreferences;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public class UserPreferences
    {
        [BsonElement("dailyWordGoal")]
        public int DailyWordGoal { get; set; }

        [BsonElement("notificationEnabled")]
        public bool NotificationEnabled { get; set; }

        [BsonElement("theme")]
        public string Theme { get; set; }

        [BsonElement("language")]
        public string Language { get; set; }

        [BsonElement("categories")]
        public List<string> PreferredCategories { get; set; }

        [BsonElement("studyReminderTime")]
        public TimeSpan? StudyReminderTime { get; set; }

        public UserPreferences()
        {
            DailyWordGoal = 10;
            NotificationEnabled = true;
            Theme = "Light";
            Language = "en-US";
            PreferredCategories = new List<string>();
        }
    }

    public enum UserRole
    {
        User = 1,
        Admin = 2
    }

    public class UserStatistics
    {
        [BsonElement("totalWords")]
        public int TotalWords { get; set; }

        [BsonElement("learnedWords")]
        public int LearnedWords { get; set; }

        [BsonElement("learningWords")]
        public int LearningWords { get; set; }

        [BsonElement("notLearnedWords")]
        public int NotLearnedWords { get; set; }

        [BsonElement("totalExams")]
        public int TotalExams { get; set; }

        [BsonElement("passedExams")]
        public int PassedExams { get; set; }

        [BsonElement("failedExams")]
        public int FailedExams { get; set; }

        [BsonElement("averageScore")]
        public double AverageScore { get; set; }

        [BsonElement("dates")]
        public List<DateTime> Dates { get; set; } = new();

        [BsonElement("correctAnswers")]
        public List<int> CorrectAnswers { get; set; } = new();

        [BsonElement("incorrectAnswers")]
        public List<int> IncorrectAnswers { get; set; } = new();

        [BsonElement("studyTime")]
        public List<double> StudyTime { get; set; } = new();

        [BsonElement("scoreOverTime")]
        public List<double> ScoreOverTime { get; set; } = new();

        [BsonElement("totalAttempts")]
        public int TotalAttempts { get; set; }

        [BsonElement("correctAttempts")]
        public int CorrectAttempts { get; set; }

        [BsonElement("currentStreak")]
        public int CurrentStreak { get; set; }

        [BsonElement("longestStreak")]
        public int LongestStreak { get; set; }

        [BsonElement("lastStudyDate")]
        public DateTime? LastStudyDate { get; set; }

        [BsonElement("achievements")]
        public List<Achievement> Achievements { get; set; } = new();

        [BsonElement("dailyStats")]
        public Dictionary<string, long> DailyStats { get; set; } = new();

        [BsonElement("weeklyStats")]
        public Dictionary<string, long> WeeklyStats { get; set; } = new();

        [BsonElement("monthlyStats")]
        public Dictionary<string, long> MonthlyStats { get; set; } = new();

        [BsonElement("overallStats")]
        public Dictionary<string, long> OverallStats { get; set; } = new();

        public void UpdateStreak()
        {
            if (LastStudyDate.HasValue && LastStudyDate.Value.Date == DateTime.UtcNow.Date.AddDays(-1))
            {
                CurrentStreak++;
                if (CurrentStreak > LongestStreak)
                {
                    LongestStreak = CurrentStreak;
                }
            }
            else if (LastStudyDate.HasValue && LastStudyDate.Value.Date != DateTime.UtcNow.Date)
            {
                CurrentStreak = 0;
            }
            LastStudyDate = DateTime.UtcNow;
        }

        public class Achievement
        {
            [BsonElement("id")]
            public string Id { get; set; }

            [BsonElement("name")]
            public string Name { get; set; }

            [BsonElement("description")]
            public string Description { get; set; }

            [BsonElement("unlockedAt")]
            public DateTime UnlockedAt { get; set; }

            [BsonElement("type")]
            public AchievementType Type { get; set; }

            public Achievement()
            {
                Id = ObjectId.GenerateNewId().ToString();
                UnlockedAt = DateTime.UtcNow;
            }
        }

        public enum AchievementType
        {
            WordCount,
            Streak,
            StudyTime,
            Category,
            Special
        }
    }

    public class UserSettings
    {
        [BsonElement("defaultLanguage")]
        public string DefaultLanguage { get; set; } = "en";

        [BsonElement("dailyWordCount")]
        public int DailyWordCount { get; set; } = 10;

        [BsonElement("notificationEnabled")]
        public bool NotificationEnabled { get; set; } = true;

        [BsonElement("theme")]
        public string Theme { get; set; } = "Light";
    }
} 