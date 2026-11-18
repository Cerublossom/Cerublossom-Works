using System;
using System.Xml.Serialization;

namespace service.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }

        [XmlIgnore]
        public Resource Resource { get; set; }

        public int UserId { get; set; }

        [XmlIgnore]
        public User User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Purpose { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }

        [XmlIgnore]
        public TimeSpan Duration => EndTime - StartTime;

        [XmlIgnore]
        public bool IsConfirmed => Status == BookingStatus.Confirmed;

        public Booking() { }
    }
}
