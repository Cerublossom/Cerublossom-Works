using System;
using System.Windows.Input;
using service.Models;

namespace service.ViewModels
{
    public class CalendarSlotViewModel : ViewModelBase
    {
        public DateTime Start { get; }
        public DateTime End { get; }
        public Booking Booking { get; }
        public bool IsBooked => Booking != null;
        public string TimeLabel => $"{Start:HH:mm}–{End:HH:mm}";
        public string Title => IsBooked ? Booking.Purpose : "Свободно";
        public string UserName => IsBooked ? (Booking.User?.DisplayName ?? Booking.UserId.ToString()) : string.Empty;

        public event Action BookSlotRequested;

        public ICommand BookCommand { get; }

        public CalendarSlotViewModel(DateTime start, DateTime end, Booking booking)
        {
            Start = start;
            End = end;
            Booking = booking;
            // Нажимать можно только на свободный слот, занятые время не открывают форму
            BookCommand = new RelayCommand(_ => BookSlotRequested?.Invoke(), _ => !IsBooked);
        }
    }
}
