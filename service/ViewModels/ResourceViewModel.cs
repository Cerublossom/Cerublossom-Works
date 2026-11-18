using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using service.Models;
using service.Services;

namespace service.ViewModels
{
    public class ResourceViewModel : ViewModelBase
    {
        private readonly BookingService _bookingService;
        private string _name;
        private string _type = "ConferenceRoom";
        private string _description;
        private User _responsible;
        private bool _isActive = true;
        private Resource _editing;
        private string _statusMessage;

        public ResourceViewModel(BookingService bookingService)
        {
            _bookingService = bookingService;

            SaveCommand = new RelayCommand(_ => Save(), _ => CanSave);
            DeleteCommand = new RelayCommand(_ => Delete(), _ => SelectedResource != null);
            EditCommand = new RelayCommand<Resource>(r => StartEdit(r), r => r != null);
            NewCommand = new RelayCommand(_ => StartEdit(null));
            CancelEditCommand = new RelayCommand(_ => CancelEdit());

            Load();
        }

        public ObservableCollection<Resource> Resources { get; } = new ObservableCollection<Resource>();

        public List<User> Users => _bookingService.GetUsers().Where(u => u.IsActive).ToList();

        public List<string> Types { get; } = new List<string>
        {
            "ConferenceRoom",
            "MeetingRoom",
            "Equipment",
            "Photographer",
            "Other"
        };

        private Resource _selectedResource;

        public Resource SelectedResource
        {
            get => _selectedResource;
            set => Set(ref _selectedResource, value);
        }

        public string Name
        {
            get => _name;
            set { Set(ref _name, value); Raise(nameof(CanSave)); }
        }

        public string Type
        {
            get => _type;
            set => Set(ref _type, value);
        }

        public string Description
        {
            get => _description;
            set => Set(ref _description, value);
        }

        public User Responsible
        {
            get => _responsible;
            set => Set(ref _responsible, value);
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

        public bool CanSave => !string.IsNullOrWhiteSpace(Name);

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand CancelEditCommand { get; }

        private void Load()
        {
            Resources.Clear();
            foreach (var r in _bookingService.GetResources().OrderBy(r => r.Name))
                Resources.Add(r);
        }

        private void StartEdit(Resource r)
        {
            _editing = r;
            Name = r?.Name ?? string.Empty;
            Type = r?.Type ?? "ConferenceRoom";
            Description = r?.Description ?? string.Empty;
            Responsible = r != null ? Users.FirstOrDefault(u => u.Id == r.ResponsiblePersonId.GetValueOrDefault()) : null;
            IsActive = r?.IsActive ?? true;
            SelectedResource = r;
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
            Name = string.Empty;
            Type = "ConferenceRoom";
            Description = string.Empty;
            Responsible = null;
            IsActive = true;
            SelectedResource = null;
        }

        private void Save()
        {
            if (_editing != null)
            {
                _editing.Name = Name;
                _editing.Type = Type;
                _editing.Description = Description;
                _editing.ResponsiblePersonId = Responsible?.Id;
                _editing.ResponsiblePerson = Responsible;
                _editing.IsActive = IsActive;
                _bookingService.UpdateResource(_editing);
            }
            else
            {
                var resource = new Resource
                {
                    Name = Name,
                    Type = Type,
                    Description = Description,
                    ResponsiblePersonId = Responsible?.Id,
                    ResponsiblePerson = Responsible,
                    IsActive = IsActive
                };
                _bookingService.AddResource(resource);
            }

            Load();
            CancelEdit();
            StatusMessage = "Ресурс сохранён.";
        }

        private void Delete()
        {
            if (SelectedResource == null)
                return;

            var hasActiveBookings = _bookingService.GetBookings()
                .Any(b => b.ResourceId == SelectedResource.Id && b.Status != BookingStatus.Cancelled);

            if (hasActiveBookings)
            {
                StatusMessage = "Не удалить: у ресурса есть активные бронирования.";
                return;
            }

            _bookingService.RemoveResource(SelectedResource);
            Load();
            CancelEdit();
            StatusMessage = "Ресурс удалён.";
        }
    }
}
