using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using service.Models;
using service.Services;

namespace service.ViewModels
{
    public class CalendarViewModel : ViewModelBase
    {
        private readonly BookingService _bookingService;
        private readonly NotificationService _notificationService;
        private DateTime _selectedDate = DateTime.Today;

        public CalendarViewModel(BookingService bookingService, NotificationService notificationService, Action<Resource, DateTime> onBookSlot)
        {
            _bookingService = bookingService;
            _notificationService = notificationService;
            OnBookSlot = onBookSlot;

            LoadCommand = new RelayCommand(_ => Load());
            RefreshCommand = new RelayCommand(_ => Load());
            NewBookingCommand = new RelayCommand(_ => NewBookingRequested?.Invoke());

            Load();
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (Set(ref _selectedDate, value))
                    Load();
            }
        }

        public ObservableCollection<ResourceCalendarViewModel> ResourceCalendars { get; } =
            new ObservableCollection<ResourceCalendarViewModel>();

        public ICommand LoadCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NewBookingCommand { get; }

        public Action<Resource, DateTime> OnBookSlot { get; }
        public event Action NewBookingRequested;

        public void Load()
        {
            ResourceCalendars.Clear();
            foreach (var r in _bookingService.GetResources().Where(r => r.IsActive).OrderBy(r => r.Name))
            {
                var vm = new ResourceCalendarViewModel(r, _bookingService.GetBookingsForResource(r.Id, SelectedDate));
                vm.BookSlotRequested += slot => OnBookSlot?.Invoke(vm.Resource, slot.Start);
                ResourceCalendars.Add(vm);
            }
        }
    }

    public class ResourceCalendarViewModel : ViewModelBase
    {
        public Resource Resource { get; }
        public ObservableCollection<CalendarSlotViewModel> Slots { get; } = new ObservableCollection<CalendarSlotViewModel>();

        public event Action<CalendarSlotViewModel> BookSlotRequested;

        public ResourceCalendarViewModel(Resource resource, List<Booking> bookings)
        {
            Resource = resource;
            var day = bookings.FirstOrDefault()?.StartTime.Date ?? DateTime.Today;

            for (var t = day.AddHours(8); t < day.AddHours(20); t = t.AddMinutes(30))
            {
                var end = t.AddMinutes(30);
                var b = bookings.FirstOrDefault(x => x.StartTime < end && x.EndTime > t);
                var slot = new CalendarSlotViewModel(t, end, b);
                slot.BookSlotRequested += () => BookSlotRequested?.Invoke(slot);
                Slots.Add(slot);
            }
        }
    }
}
