using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using service.Models;
using service.Services;

namespace service.ViewModels
{
    public class UserViewModel : ViewModelBase
    {
        private readonly BookingService _bookingService;
        private User _editing;
        private User _selectedUser;
        private string _fullName;
        private string _email;
        private string _role = "Employee";
        private bool _isActive = true;
        private bool _isEditing;
        private string _statusMessage;
        private string _searchText;
        private string _sortMode = "По алфавиту";
        private DateTime _selectedDate = DateTime.Today;

        public UserViewModel(BookingService bookingService)
        {
            _bookingService = bookingService;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave);
            DeleteCommand = new RelayCommand<User>(u =>
            {
                SelectedUser = u;
                Delete();
            }, u => u != null);
            EditCommand = new RelayCommand<User>(u => StartEdit(u));
            NewCommand = new RelayCommand(_ => StartEdit(null));
            CancelEditCommand = new RelayCommand(_ => CancelEdit());

            Load();
        }

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        // Бронирования на выбранный день — для панели "Активные приглашения"
        public ObservableCollection<Booking> DayBookings { get; } = new ObservableCollection<Booking>();

        public List<string> Roles { get; } = new List<string> { "Administrator", "Employee" };

        public List<string> SortModes { get; } = new List<string> { "По алфавиту", "По роли" };

        public User SelectedUser
        {
            get => _selectedUser;
            set => Set(ref _selectedUser, value);
        }

        public string FullName
        {
            get => _fullName;
            set { Set(ref _fullName, value); Raise(nameof(CanSave)); }
        }

        public string Email
        {
            get => _email;
            set { Set(ref _email, value); Raise(nameof(CanSave)); }
        }

        public string Role
        {
            get => _role;
            set => Set(ref _role, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => Set(ref _isActive, value);
        }

        // Поиск по ФИО и email в верхней панели
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (Set(ref _searchText, value))
                    Load();
            }
        }

        public string SortMode
        {
            get => _sortMode;
            set
            {
                if (Set(ref _sortMode, value))
                    Load();
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (Set(ref _selectedDate, value))
                    LoadDayBookings();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => Set(ref _isEditing, value);
        }

        public bool CanSave => !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Email);

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand CancelEditCommand { get; }

        private void Load()
        {
            var query = _bookingService.GetUsers().AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var text = SearchText.ToLowerInvariant();
                query = query.Where(u =>
                    (u.FullName ?? string.Empty).ToLowerInvariant().Contains(text) ||
                    (u.Email ?? string.Empty).ToLowerInvariant().Contains(text));
            }

            if (SortMode == "По роли")
                query = query.OrderBy(u => u.Role).ThenBy(u => u.FullName);
            else
                query = query.OrderBy(u => u.FullName);

            Users.Clear();
            foreach (var u in query)
                Users.Add(u);

            LoadDayBookings();
        }

        private void LoadDayBookings()
        {
            DayBookings.Clear();
            foreach (var b in _bookingService.GetBookings()
                         .Where(b => b.StartTime.Date == SelectedDate.Date &&
                                     b.Status != BookingStatus.Cancelled)
                         .OrderBy(b => b.StartTime))
            {
                DayBookings.Add(b);
            }
        }

        private void StartEdit(User u)
        {
            _editing = u;
            SelectedUser = u;
            FullName = u?.FullName ?? string.Empty;
            Email = u?.Email ?? string.Empty;
            Role = u?.Role ?? "Employee";
            IsActive = u?.IsActive ?? true;
            StatusMessage = string.Empty;
            IsEditing = true;
        }

        private void CancelEdit()
        {
            _editing = null;
            IsEditing = false;
            ClearForm();
        }

        private void ClearForm()
        {
            FullName = string.Empty;
            Email = string.Empty;
            Role = "Employee";
            IsActive = true;
            SelectedUser = null;
        }

        private void Save()
        {
            if (_editing != null)
            {
                _editing.FullName = FullName;
                _editing.Email = Email;
                _editing.Role = Role;
                _editing.IsActive = IsActive;
                _bookingService.UpdateUser(_editing);
            }
            else
            {
                var user = new User
                {
                    FullName = FullName,
                    Email = Email,
                    Role = Role,
                    IsActive = IsActive
                };
                _bookingService.AddUser(user);
            }

            Load();
            CancelEdit();
            StatusMessage = "Пользователь сохранён.";
        }

        private void Delete()
        {
            if (SelectedUser == null)
                return;

            var hasBookings = _bookingService.GetBookings()
                .Any(b => b.UserId == SelectedUser.Id && b.Status != BookingStatus.Cancelled);

            if (hasBookings)
            {
                StatusMessage = "Не удалить: у пользователя есть активные бронирования.";
                return;
            }

            _bookingService.RemoveUser(SelectedUser);
            Load();
            CancelEdit();
            StatusMessage = "Пользователь удалён.";
        }
    }
}
