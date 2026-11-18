using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using service.Models;
using service.Services;

namespace service.ViewModels
{
    public class BookingViewModel : ViewModelBase
    {
        private readonly BookingService _bookingService;
        private readonly NotificationService _notificationService;
        private Resource _selectedResource;
        private User _selectedUser;
        private DateTime _selectedDate = DateTime.Today;
        private string _startTime = "09:00";
        private string _endTime = "10:00";
        private string _purpose;
        private string _statusMessage;

        public BookingViewModel(BookingService bookingService, NotificationService notificationService)
        {
            _bookingService = bookingService;
            _notificationService = notificationService;
            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave);
            CancelCommand = new RelayCommand(_ => CancelRequested?.Invoke());

            var slots = new List<string>();
            for (var i = 8; i <= 20; i++)
                slots.Add(new DateTime(1, 1, 1, i, 0, 0).ToString("HH:mm"));
            TimeSlots = slots;
        }

        public event Action CancelRequested;
        public event Action<Booking> BookingCreated;

        public List<string> TimeSlots { get; }

        public List<Resource> Resources => _bookingService.GetResources().Where(r => r.IsActive).ToList();

        public List<User> Users => _bookingService.GetUsers().Where(u => u.IsActive).ToList();

        public Resource SelectedResource
        {
            get => _selectedResource;
            set { Set(ref _selectedResource, value); Validate(); }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set { Set(ref _selectedUser, value); Validate(); }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { Set(ref _selectedDate, value); Validate(); }
        }

        public string StartTime
        {
            get => _startTime;
            set { Set(ref _startTime, value); Validate(); }
        }

        public string EndTime
        {
            get => _endTime;
            set { Set(ref _endTime, value); Validate(); }
        }

        public string Purpose
        {
            get => _purpose;
            set => Set(ref _purpose, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public bool CanSave =>
            SelectedResource != null &&
            SelectedUser != null &&
            !string.IsNullOrWhiteSpace(StartTime) &&
            !string.IsNullOrWhiteSpace(EndTime);

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public void PrepareNew(Resource resource, DateTime date)
        {
            SelectedResource = resource;
            SelectedDate = date;
            StartTime = date.AddHours(9).ToString("HH:mm");
            EndTime = date.AddHours(10).ToString("HH:mm");
            SelectedUser = Users.FirstOrDefault();
            Purpose = string.Empty;
            StatusMessage = string.Empty;
        }

        public void PrepareNewForDate(DateTime date)
        {
            SelectedResource = Resources.FirstOrDefault();
            SelectedDate = date;
            StartTime = date.AddHours(9).ToString("HH:mm");
            EndTime = date.AddHours(10).ToString("HH:mm");
            SelectedUser = Users.FirstOrDefault();
            Purpose = string.Empty;
            StatusMessage = string.Empty;
        }

        private void Validate()
        {
            StatusMessage = string.Empty;
            if (!CanSave)
                return;

            DateTime start, end;
            if (!DateTime.TryParse($"{SelectedDate:yyyy-MM-dd} {StartTime}", out start) ||
                !DateTime.TryParse($"{SelectedDate:yyyy-MM-dd} {EndTime}", out end))
            {
                StatusMessage = "Некорректное время.";
                return;
            }

            if (end <= start)
            {
                StatusMessage = "Время окончания должно быть позже времени начала.";
                return;
            }

            if (!_bookingService.IsResourceAvailable(SelectedResource.Id, start, end))
                StatusMessage = "Внимание: выбранное время уже занято — возможна ошибка повторного бронирования.";
        }

        private void Save()
        {
            var start = DateTime.Parse($"{SelectedDate:yyyy-MM-dd} {StartTime}");
            var end = DateTime.Parse($"{SelectedDate:yyyy-MM-dd} {EndTime}");

            try
            {
                var booking = _bookingService.CreateBooking(
                    SelectedResource.Id,
                    SelectedUser.Id,
                    start,
                    end,
                    Purpose,
                    SelectedUser.DisplayName);

                // Автоматическое уведомление всех задействованных сотрудников о новом бронированию.
                var participants = _bookingService.GetUsers().Where(u => u.IsActive).ToList();
                var responsible = _bookingService.GetResources()
                    .FirstOrDefault(r => r.Id == SelectedResource.Id)?.ResponsiblePerson;
                if (responsible != null && !participants.Any(u => u.Id == responsible.Id))
                    participants.Add(responsible);

                foreach (var u in participants)
                {
                    _notificationService.Add(
                        u.Id,
                        $"Новое бронирование: ресурс \"{SelectedResource.Name}\", {start:dd.MM.yyyy} {start:HH:mm}–{end:HH:mm}. Цель: {Purpose}.",
                        NotificationType.BookingCreated,
                        booking.Id);

                    // Синхронизация бронирований с календарями пользователей.
                    _notificationService.Add(
                        u.Id,
                        $"Синхронизировано с календарем: \"{SelectedResource.Name}\" {start:dd.MM.yyyy} {start:HH:mm}–{end:HH:mm}.",
                        NotificationType.CalendarSync,
                        booking.Id);
                }

                StatusMessage = "Бронирование успешно создано. Участники уведомлены, календарь синхронизирован.";
                BookingCreated?.Invoke(booking);
            }
            catch (ConflictException ex)
            {
                StatusMessage = $"Ошибка повторного бронирования: {ex.Message}";
                _notificationService.Add(
                    SelectedUser?.Id ?? 0,
                    $"Конфликт времени бронирования: {ex.Message}",
                    NotificationType.Conflict);
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка при создании бронирования: " + ex.Message;
            }
        }
    }
}
