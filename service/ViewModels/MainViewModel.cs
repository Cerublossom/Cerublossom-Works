using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using service.Models;
using service.Services;
using service.Views;

namespace service.ViewModels
{
    // Главная view-модель приложения. Управляет навигацией между окнами.
    public class MainViewModel : ViewModelBase
    {
        private readonly DataService _dataService;      // работа с XML-файлами данных
        private readonly BookingService _bookingService; // логика бронирования
        private readonly NotificationService _notificationService; // уведомления
        private object _currentView;                    // текущее отображаемое окно
        private User _currentUser;                      // текущий пользователь

        // окна приложения
        private readonly CalendarView _calendarView;
        private readonly BookingView _bookingView;
        private readonly ResourceView _resourceView;
        private readonly NotificationsView _notificationsView;
        private readonly UserView _userView;

        public MainViewModel()
        {
            _dataService = new DataService();
            _bookingService = new BookingService(_dataService);
            _notificationService = new NotificationService(_dataService);

            SeedData(); // заполняем демо-данными при первом запуске

            _currentUser = _bookingService.GetUsers().FirstOrDefault(u => u.Role == "Administrator")
                           ?? _bookingService.GetUsers().FirstOrDefault()
                           ?? new User(1, "Пользователь", "user@company.com");

            CalendarViewVm = new CalendarViewModel(_bookingService, _notificationService, OnBookSlotRequested);
            BookingViewVm = new BookingViewModel(_bookingService, _notificationService);
            ResourceViewVm = new ResourceViewModel(_bookingService);
            NotificationsViewVm = new NotificationsViewModel(_notificationService, _bookingService, _currentUser.Id);
            UserViewVm = new UserViewModel(_bookingService);

            _calendarView = new CalendarView { DataContext = CalendarViewVm };
            _bookingView = new BookingView { DataContext = BookingViewVm };
            _resourceView = new ResourceView { DataContext = ResourceViewVm };
            _notificationsView = new NotificationsView { DataContext = NotificationsViewVm };
            _userView = new UserView { DataContext = UserViewVm };

            CalendarViewVm.NewBookingRequested += () =>
            {
                BookingViewVm.PrepareNewForDate(CalendarViewVm.SelectedDate);
                CurrentView = _bookingView;
            };
            BookingViewVm.BookingCreated += b =>
            {
                _notificationService.Add(
                    _currentUser.Id,
                    $"Ваше бронирование подтверждено: \"{b.Resource.Name}\" {b.StartTime:dd.MM.yyyy HH:mm}–{b.EndTime:HH:mm}.",
                    NotificationType.BookingCreated,
                    b.Id);
                CurrentView = _calendarView;
            };
            BookingViewVm.CancelRequested += () => CurrentView = _calendarView;

            CurrentView = _calendarView;

            ShowCalendarCommand = new RelayCommand(_ => CurrentView = _calendarView);
            ShowBookingCommand = new RelayCommand(_ => CurrentView = _bookingView);
            ShowResourcesCommand = new RelayCommand(_ => CurrentView = _resourceView);
            ShowNotificationsCommand = new RelayCommand(_ => CurrentView = _notificationsView);
            ShowUsersCommand = new RelayCommand(_ => CurrentView = _userView);
        }

        public object CurrentView
        {
            get => _currentView;
            set => Set(ref _currentView, value);
        }

        public User CurrentUser
        {
            get => _currentUser;
            set => Set(ref _currentUser, value);
        }

        public CalendarViewModel CalendarViewVm { get; }
        public BookingViewModel BookingViewVm { get; }
        public ResourceViewModel ResourceViewVm { get; }
        public NotificationsViewModel NotificationsViewVm { get; }
        public UserViewModel UserViewVm { get; }

        public ICommand ShowCalendarCommand { get; }
        public ICommand ShowBookingCommand { get; }
        public ICommand ShowResourcesCommand { get; }
        public ICommand ShowNotificationsCommand { get; }
        public ICommand ShowUsersCommand { get; }

        private void OnBookSlotRequested(Resource resource, DateTime date)
        {
            BookingViewVm.PrepareNew(resource, date);
            CurrentView = _bookingView;
        }

        private void SeedData()
        {
            if (!_bookingService.GetUsers().Any())
            {
                var users = new List<User>
                {
                    new User(1, "Иван Иванов", "ivanov@company.com", "Administrator"),
                    new User(2, "Мария Петрова", "petrova@company.com", "Employee"),
                    new User(3, "Алексей Сидоров", "sidorov@company.com", "Employee"),
                    new User(4, "Ольга Кузнецова", "kuznetsova@company.com", "Employee")
                };
                _dataService.Save("users.xml", users);
            }

            if (!_bookingService.GetResources().Any())
            {
                var users = _bookingService.GetUsers();
                var resources = new List<Resource>
                {
                    new Resource { Id = 1, Name = "Конференц-зал А", Type = "ConferenceRoom", Description = "Переговорка на 12 человек, проектор, доска", ResponsiblePersonId = users.FirstOrDefault(u => u.Id == 1)?.Id, IsActive = true },
                    new Resource { Id = 2, Name = "Конференц-зал Б", Type = "ConferenceRoom", Description = "Малый переговорный зал на 6 человек", ResponsiblePersonId = users.FirstOrDefault(u => u.Id == 2)?.Id, IsActive = true },
                    new Resource { Id = 3, Name = "Проектор Epson", Type = "Equipment", Description = "Переносной проектор для презентаций", ResponsiblePersonId = users.FirstOrDefault(u => u.Id == 3)?.Id, IsActive = true },
                    new Resource { Id = 4, Name = "Фотограф (студия)", Type = "Photographer", Description = "Услуги корпоративного фотографа по бронированию слота", ResponsiblePersonId = users.FirstOrDefault(u => u.Id == 1)?.Id, IsActive = true },
                    new Resource { Id = 5, Name = "Ноутбук Dell", Type = "Equipment", Description = "Ноутбук для встреч и презентаций", ResponsiblePersonId = users.FirstOrDefault(u => u.Id == 2)?.Id, IsActive = true },
                    new Resource { Id = 6, Name = "Переговорка \"Зеленая\"", Type = "MeetingRoom", Description = "Уютная переговорка на 4 человека", ResponsiblePersonId = users.FirstOrDefault(u => u.Id == 4)?.Id, IsActive = true }
                };
                foreach (var r in resources)
                    _bookingService.AddResource(r);
            }
        }
    }
}
