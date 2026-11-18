using System;
using System.Collections.Generic;
using System.Linq;
using service.Models;

namespace service.Services
{
    public class NotificationService
    {
        private readonly DataService _data;
        private List<Notification> _notifications;
        private bool _loaded;

        public NotificationService(DataService data)
        {
            _data = data;
        }

        private List<Notification> Load()
        {
            if (!_loaded)
            {
                _notifications = _data.Load<List<Notification>>("notifications.xml") ?? new List<Notification>();
                _loaded = true;
            }
            return _notifications;
        }

        private void Save()
        {
            _data.Save("notifications.xml", _notifications);
        }

        private int NextId()
        {
            return _notifications.Any() ? _notifications.Max(n => n.Id) + 1 : 1;
        }

        public void Add(int userId, string message, NotificationType type, int? relatedBookingId = null)
        {
            var notification = new Notification
            {
                Id = NextId(),
                UserId = userId,
                Message = message ?? string.Empty,
                Type = type,
                RelatedBookingId = relatedBookingId
            };
            Load().Add(notification);
            Save();
        }

        public List<Notification> GetForUser(int userId)
        {
            return Load().Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToList();
        }

        public List<Notification> GetAll()
        {
            return Load().OrderByDescending(n => n.CreatedAt).ToList();
        }

        public void MarkRead(int id)
        {
            var n = Load().FirstOrDefault(x => x.Id == id);
            if (n != null)
            {
                n.IsRead = true;
                Save();
            }
        }

        public int UnreadCount(int userId)
        {
            return Load().Count(n => n.UserId == userId && !n.IsRead);
        }
    }
}
