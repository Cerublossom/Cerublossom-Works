using System;
using System.Xml.Serialization;

namespace service.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; }
        public int? RelatedBookingId { get; set; }

        [XmlIgnore]
        public string TypeLabel => Type.ToString();

        public Notification() { }
    }
}
