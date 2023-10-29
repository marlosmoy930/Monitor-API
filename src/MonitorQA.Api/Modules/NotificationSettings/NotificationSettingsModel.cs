using MonitorQA.Data.Entities;
using System;

namespace MonitorQA.Api.Modules.NotificationSettings
{
    public class NotificationSettingsModel
    {
        public NotificationEventType NotificationEventType { get; set; }

        public InteractionType InteractionType { get; set; }

        public bool IsEnabled { get; set; }

        public static NotificationSettingsModel CreateModel(NotificationSetting entity)
        {
            return new NotificationSettingsModel
            {
                NotificationEventType = entity.NotificationEventType,
                InteractionType = entity.InteractionType,
                IsEnabled = entity.IsEnabled
            };
        }

        public NotificationSetting CreateEntity(Guid userId)
        {
            return new NotificationSetting
            {
                UserId = userId,
                NotificationEventType = NotificationEventType,
                InteractionType = InteractionType,
                IsEnabled = IsEnabled,
            };
        }
    }
}
