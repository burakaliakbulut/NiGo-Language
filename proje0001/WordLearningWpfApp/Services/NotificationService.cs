using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using WordLearningWpfApp.Models;
using MaterialDesignThemes.Wpf;

namespace WordLearningWpfApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IMongoCollection<Notification> _notifications;
        private readonly IMongoCollection<NotificationSettings> _settings;
        private readonly Dictionary<string, NotificationSettings> _settingsCache;
        private readonly UserService _userService;
        private readonly Snackbar _snackbar;

        public NotificationService(IMongoDatabase dbContext, UserService userService, Snackbar snackbar)
        {
            _notifications = dbContext.GetCollection<Notification>("notifications");
            _settings = dbContext.GetCollection<NotificationSettings>("notification_settings");
            _settingsCache = new Dictionary<string, NotificationSettings>();
            _userService = userService;
            _snackbar = snackbar ?? throw new ArgumentNullException(nameof(snackbar));
        }

        public async Task<List<Notification>> GetNotificationsAsync(string userId)
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
            var sort = Builders<Notification>.Sort.Descending(n => n.CreatedAt);
            return await _notifications.Find(filter).Sort(sort).ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(string notificationId)
        {
            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);

            var result = await _notifications.UpdateOneAsync(n => n.Id == notificationId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId)
        {
            var result = await _notifications.DeleteOneAsync(n => n.Id == notificationId);
            return result.DeletedCount > 0;
        }

        public async Task<NotificationSettings> GetSettingsAsync(string userId)
        {
            if (_settingsCache.TryGetValue(userId, out var cachedSettings))
            {
                return cachedSettings;
            }

            var filter = Builders<NotificationSettings>.Filter.Eq(s => s.UserId, userId);
            var settings = await _settings.Find(filter).FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new NotificationSettings
                {
                    UserId = userId,
                    EmailEnabled = true,
                    PushEnabled = true,
                    DailyReminderEnabled = true,
                    WeeklyReportEnabled = true
                };
                await _settings.InsertOneAsync(settings);
            }

            _settingsCache[userId] = settings;
            return settings;
        }

        public async Task<bool> UpdateSettingsAsync(NotificationSettings settings)
        {
            var filter = Builders<NotificationSettings>.Filter.Eq(s => s.UserId, settings.UserId);
            var result = await _settings.ReplaceOneAsync(filter, settings, new ReplaceOptions { IsUpsert = true });

            if (result.IsAcknowledged)
            {
                _settingsCache[settings.UserId] = settings;
                return true;
            }
            return false;
        }

        public async Task<bool> CreateNotificationAsync(Notification notification)
        {
            try
            {
                await _notifications.InsertOneAsync(notification);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ShowError(string message)
        {
            System.Windows.MessageBox.Show(message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        public void ShowSuccess(string message)
        {
            System.Windows.MessageBox.Show(message, "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        public void ShowWarning(string message)
        {
            System.Windows.MessageBox.Show(message, "Warning", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }

        public void ShowInfo(string message)
        {
            System.Windows.MessageBox.Show(message, "Information", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        public bool ShowConfirmation(string message)
        {
            var result = System.Windows.MessageBox.Show(message, "Confirmation", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            return result == System.Windows.MessageBoxResult.Yes;
        }

        public async Task ShowErrorAsync(string title, string message)
        {
            await Task.Run(() => System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error));
        }

        public async Task ShowSuccessAsync(string message)
        {
            await Task.Run(() => ShowSuccess(message));
        }

        public async Task ShowSuccessAsync(string title, string message)
        {
            await Task.Run(() => ShowSuccess($"{title}: {message}"));
        }

        public async Task<bool> ShowConfirmationAsync(string title, string message)
        {
            var result = await Task.Run(() => System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question));
            return result == System.Windows.MessageBoxResult.Yes;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool includeRead = false)
        {
            var filter = includeRead
                ? Builders<Notification>.Filter.Eq(n => n.UserId, userId)
                : Builders<Notification>.Filter.And(
                    Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                    Builders<Notification>.Filter.Eq(n => n.IsRead, false)
                );

            return await _notifications.Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        private void ShowMessage(string message, bool isError)
        {
            if (_snackbar != null)
            {
                _snackbar.MessageQueue?.Enqueue(
                    message,
                    null,
                    null,
                    null,
                    isError,
                    true,
                    TimeSpan.FromSeconds(3)
                );
            }
        }

        public async Task ShowErrorAsync(string message) => await Task.Run(() => ShowError(message));
        public async Task ShowErrorAsync(string message, Exception ex) => await Task.Run(() => ShowError($"{message}: {ex.Message}"));
        public async Task ShowWarningAsync(string message) => await Task.Run(() => ShowWarning(message));
        public async Task ShowInfoAsync(string message) => await Task.Run(() => ShowInfo(message));
        public async Task<bool> ShowConfirmationAsync(string message) => await Task.Run(() => ShowConfirmation(message));
    }

    public class Notification
    {
        public string Id { get; set; } = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public enum NotificationType
    {
        DailyReminder,
        StreakReminder,
        Achievement,
        System,
        Custom
    }
} 