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
        private string _statusMessage;

        public UserViewModel(BookingService bookingService)
        {
            _bookingService = bookingService;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedUser != null);
            EditCommand = new RelayCommand<User>(u => StartEdit(u));
            NewCommand = new RelayCommand(_ => StartEdit(null));
            CancelEditCommand = new RelayCommand(_ => CancelEdit());

            Load();
        }

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        public List<string> Roles { get; } = new List<string> { "Administrator", "Employee" };

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

        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public bool IsEditing => _editing != null;

        public bool CanSave => !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Email);

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand CancelEditCommand { get; }

        private void Load()
        {
            Users.Clear();
            foreach (var u in _bookingService.GetUsers().OrderBy(u => u.FullName))
                Users.Add(u);
        }

        private void StartEdit(User u)
        {
            _editing = u;
            SelectedUser = u;
            FullName = u?.FullName ?? string.Empty;
            Email = u?.Email ?? string.Empty;
            Role = u?.Role ?? "Employee";
            IsActive = u?.IsActive ?? true;
            Raise(nameof(IsEditing));
        }

        private void CancelEdit()
        {
            _editing = null;
            ClearForm();
            Raise(nameof(IsEditing));
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
