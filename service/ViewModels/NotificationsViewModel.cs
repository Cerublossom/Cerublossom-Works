using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using service.Models;
using service.Services;

namespace service.ViewModels
{
    public class NotificationsViewModel : ViewModelBase
    {
        private readonly NotificationService _notificationService;
        private readonly BookingService _bookingService;
        private int _userId;
        private User _selectedUser;

        public NotificationsViewModel(NotificationService notificationService, BookingService bookingService, int defaultUserId)
        {
            _notificationService = notificationService;
            _bookingService = bookingService;
            _userId = defaultUserId;

            MarkReadCommand = new RelayCommand<int>(id => { _notificationService.MarkRead(id); Load(); });
            RefreshCommand = new RelayCommand(_ => Load());

            Load();
        }

        public List<User> Users => _bookingService.GetUsers().Where(u => u.IsActive).ToList();

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                Set(ref _selectedUser, value);
                _userId = value?.Id ?? 0;
                Load();
            }
        }

        public int UserId
        {
            get => _userId;
            set { Set(ref _userId, value); Load(); }
        }

        public ObservableCollection<NotificationItemViewModel> Items { get; } =
            new ObservableCollection<NotificationItemViewModel>();

        public int UnreadCount => Items.Count(i => !i.IsRead);

        public ICommand MarkReadCommand { get; }
        public ICommand RefreshCommand { get; }

        private void Load()
        {
            Items.Clear();
            var list = _userId == 0
                ? _notificationService.GetAll()
                : _notificationService.GetForUser(_userId);

            foreach (var n in list)
                Items.Add(new NotificationItemViewModel(n, _notificationService, _bookingService));

            Raise(nameof(UnreadCount));
        }
    }

    public class NotificationItemViewModel : ViewModelBase
    {
        private readonly Notification _notification;
        private readonly NotificationService _service;
        private readonly BookingService _bookingService;

        public NotificationItemViewModel(Notification notification, NotificationService service, BookingService bookingService)
        {
            _notification = notification;
            _service = service;
            _bookingService = bookingService;
        }

        public int Id => _notification.Id;

        public string Message => _notification.Message;

        public string Time => _notification.CreatedAt.ToString("dd.MM.yyyy HH:mm");

        public string TypeLabel => _notification.TypeLabel;

        public string UserName
        {
            get
            {
                var u = _bookingService.GetUsers().FirstOrDefault(x => x.Id == _notification.UserId);
                return u != null ? u.DisplayName : $"Пользователь #{_notification.UserId}";
            }
        }

        public bool IsRead
        {
            get => _notification.IsRead;
            set
            {
                _notification.IsRead = value;
                _service.MarkRead(_notification.Id);
                Raise();
            }
        }

        public ICommand MarkReadCommand => new RelayCommand(_ =>
        {
            IsRead = true;
        });
    }
}
