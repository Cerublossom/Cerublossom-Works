namespace service.Models
{
    public enum BookingStatus
    {
        Confirmed,
        Cancelled,
        Rejected
    }

    public enum NotificationType
    {
        BookingCreated,
        Conflict,
        CalendarSync,
        Reminder
    }
}
