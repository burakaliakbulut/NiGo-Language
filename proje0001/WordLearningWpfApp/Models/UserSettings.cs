namespace WordLearningWpfApp.Models
{
    public class UserSettings
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DailyWordLimit { get; set; } = 10;
        public bool EnableNotifications { get; set; } = true;
        public bool EnableSound { get; set; } = true;
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "en-US";
    }
} 