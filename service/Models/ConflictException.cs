using System;

namespace service.Models
{
    // Исключение при попытке повторного бронирования (конфликт времени)
    public class ConflictException : Exception
    {
        public Booking ConflictingBooking { get; }

        public ConflictException(string message, Booking conflicting = null)
            : base(message)
        {
            ConflictingBooking = conflicting;
        }
    }
}
