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

        public MainViewModel(User currentUser = null)
        {
            _dataService = new DataService();
            _bookingService = new BookingService(_dataService);
            _notificationService = new NotificationService(_dataService);

            _bookingService.EnsureSeed(); // заполняем демо-данными при первом запуске

            _currentUser = currentUser
                           ?? _bookingService.GetUsers().FirstOrDefault(u => u.Role == "Administrator")
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
                BookingViewVm.PrepareNewForDate(CalendarViewVm.SelectedDate, _currentUser);
                CurrentView = _bookingView;
            };
            BookingViewVm.BookingCreated += b =>
            {
                _notificationService.Add(
                    _currentUser.Id,
                    $"Ваше бронирование подтверждено: \"{b.Resource.Name}\" {b.StartTime:dd.MM.yyyy HH:mm}–{b.EndTime:HH:mm}.",
                    NotificationType.BookingCreated,
                    b.Id);
                CalendarViewVm.Load(); // обновляем слоты, чтобы новое бронирование сразу было видно
                CurrentView = _calendarView;
            };
            BookingViewVm.CancelRequested += () => CurrentView = _calendarView;

            CurrentView = _calendarView;

            ShowCalendarCommand = new RelayCommand(_ => CurrentView = _calendarView);
            ShowBookingCommand = new RelayCommand(_ =>
            {
                BookingViewVm.PrepareNewForDate(CalendarViewVm.SelectedDate, _currentUser);
                CurrentView = _bookingView;
            });
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

        // Фото для шапки приложения (зал переговоров, как в макете)
        public string HeaderImage => DemoImages.RoomA;

        public ICommand ShowCalendarCommand { get; }
        public ICommand ShowBookingCommand { get; }
        public ICommand ShowResourcesCommand { get; }
        public ICommand ShowNotificationsCommand { get; }
        public ICommand ShowUsersCommand { get; }

        private void OnBookSlotRequested(Resource resource, DateTime start, DateTime end)
        {
            BookingViewVm.PrepareNew(resource, start, end, _currentUser);
            CurrentView = _bookingView;
        }
    }
}
