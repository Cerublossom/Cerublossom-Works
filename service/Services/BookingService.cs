using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using service.Models;

namespace service.Services
{
    // Основной сервис бронирования. Загружает/сохраняет данные в XML, проверяет конфликты времени.
    public class BookingService
    {
        private readonly DataService _data;
        private List<User> _users;
        private List<Resource> _resources;
        private List<Booking> _bookings;
        private bool _loaded;

        public BookingService(DataService data)
        {
            _data = data;
        }

        // Загружает данные из XML-файлов (один раз при создании сервиса)
        private void Load()
        {
            if (_loaded)
                return;

            _users = _data.Load<List<User>>("users.xml") ?? new List<User>();
            _resources = _data.Load<List<Resource>>("resources.xml") ?? new List<Resource>();
            _bookings = _data.Load<List<Booking>>("bookings.xml") ?? new List<Booking>();
            WireReferences();
            _loaded = true;
        }

        private void Persist()
        {
            _data.Save("users.xml", _users);
            _data.Save("resources.xml", _resources);
            _data.Save("bookings.xml", _bookings);
        }

        private void WireReferences()
        {
            foreach (var r in _resources)
                r.ResponsiblePerson = _users.FirstOrDefault(u => u.Id == r.ResponsiblePersonId.GetValueOrDefault());

            foreach (var b in _bookings)
            {
                b.Resource = _resources.FirstOrDefault(x => x.Id == b.ResourceId);
                b.User = _users.FirstOrDefault(x => x.Id == b.UserId);
            }
        }

        public List<User> GetUsers()
        {
            Load();
            return _users;
        }

        public List<Resource> GetResources()
        {
            Load();
            return _resources;
        }

        public List<Booking> GetBookings()
        {
            Load();
            return _bookings;
        }

        public List<Booking> GetBookingsForDate(DateTime date)
        {
            var day = date.Date;
            Load();
            return _bookings
                .Where(b => b.StartTime.Date == day && b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.StartTime)
                .ToList();
        }

        public List<Booking> GetBookingsForResource(int resourceId, DateTime date)
        {
            var day = date.Date;
            Load();
            return _bookings
                .Where(b => b.ResourceId == resourceId && b.StartTime.Date == day && b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.StartTime)
                .ToList();
        }

        // Проверка: свободен ли ресурс на указанный интервал (защита от двойного бронирования)
        public bool IsResourceAvailable(int resourceId, DateTime start, DateTime end, int excludeBookingId = 0)
        {
            Load();
            return !_bookings.Any(b =>
                b.ResourceId == resourceId &&
                b.Id != excludeBookingId &&
                b.Status != BookingStatus.Cancelled &&
                b.StartTime < end &&
                b.EndTime > start);
        }

        public Booking CreateBooking(int resourceId, int userId, DateTime start, DateTime end, string purpose, string createdBy)
        {
            if (end <= start)
                throw new ArgumentException("Время окончания должно быть позже времени начала.");

            if (start < DateTime.Now.AddMinutes(-30))
                throw new ArgumentException("Нельзя создать бронирование на время в прошлом.");

            Load();
            var resource = _resources.FirstOrDefault(r => r.Id == resourceId);
            if (resource == null || !resource.IsActive)
                throw new ArgumentException("Ресурс не найден или недоступен для бронирования.");

            var conflict = _bookings.FirstOrDefault(b =>
                b.ResourceId == resourceId &&
                b.Status != BookingStatus.Cancelled &&
                b.StartTime < end &&
                b.EndTime > start);

            if (conflict != null)
                throw new ConflictException(
                    $"Ресурс \"{resource.Name}\" уже занят на {conflict.StartTime:HH:mm}–{conflict.EndTime:HH:mm} ({conflict.User?.DisplayName ?? conflict.UserId.ToString()}).",
                    conflict);

            var booking = new Booking
            {
                Id = NextBookingId(),
                ResourceId = resourceId,
                UserId = userId,
                StartTime = start,
                EndTime = end,
                Purpose = purpose ?? string.Empty,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy
            };

            _bookings.Add(booking);
            Persist();
            WireReferences();
            return booking;
        }

        public void CancelBooking(int id)
        {
            Load();
            var b = _bookings.FirstOrDefault(x => x.Id == id);
            if (b != null)
            {
                b.Status = BookingStatus.Cancelled;
                Persist();
            }
        }

        public List<Resource> GetAvailableResources(DateTime start, DateTime end)
        {
            Load();
            return _resources.Where(r => r.IsActive && IsResourceAvailable(r.Id, start, end)).ToList();
        }

        public void AddResource(Resource resource)
        {
            Load();
            if (resource.Id == 0)
                resource.Id = NextResourceId();
            _resources.Add(resource);
            Persist();
            WireReferences();
        }

        public void UpdateResource(Resource resource)
        {
            Load();
            var existing = _resources.FirstOrDefault(x => x.Id == resource.Id);
            if (existing == null)
                return;
            existing.Name = resource.Name;
            existing.Type = resource.Type;
            existing.Description = resource.Description;
            existing.ResponsiblePersonId = resource.ResponsiblePersonId;
            existing.ResponsiblePerson = resource.ResponsiblePerson;
            existing.IsActive = resource.IsActive;
            Persist();
            WireReferences();
        }

        public void RemoveResource(Resource resource)
        {
            Load();
            _resources.Remove(resource);
            Persist();
        }

        public void AddUser(User user)
        {
            Load();
            if (user.Id == 0)
                user.Id = NextUserId();
            _users.Add(user);
            Persist();
        }

        public void UpdateUser(User user)
        {
            Load();
            var existing = _users.FirstOrDefault(x => x.Id == user.Id);
            if (existing == null)
                return;
            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.Role = user.Role;
            existing.IsActive = user.IsActive;
            Persist();
        }

        public void RemoveUser(User user)
        {
            Load();
            var hasBookings = _bookings.Any(b => b.UserId == user.Id && b.Status != BookingStatus.Cancelled);
            if (hasBookings)
                return; // не удалить, если есть активные бронирования
            _users.Remove(user);
            Persist();
        }

        // Заполняем демо-данными при первом запуске (пользователи и ресурсы)
        public void EnsureSeed()
        {
            Load();

            if (!_users.Any())
            {
                _users.Add(new User(1, "Иван Иванов", "ivanov@company.com", "Administrator"));
                _users.Add(new User(2, "Мария Петрова", "petrova@company.com", "Employee"));
                _users.Add(new User(3, "Алексей Сидоров", "sidorov@company.com", "Employee"));
                _users.Add(new User(4, "Ольга Кузнецова", "kuznetsova@company.com", "Employee"));
            }

            if (!_resources.Any())
            {
                _resources.Add(new Resource { Id = 1, Name = "Конференц-зал А", Type = "ConferenceRoom", Description = "Переговорка на 12 человек, проектор, доска", ImagePath = DemoImages.RoomA, ResponsiblePersonId = _users.FirstOrDefault(u => u.Id == 1)?.Id, IsActive = true });
                _resources.Add(new Resource { Id = 2, Name = "Конференц-зал Б", Type = "ConferenceRoom", Description = "Малый переговорный зал на 6 человек", ImagePath = DemoImages.RoomB, ResponsiblePersonId = _users.FirstOrDefault(u => u.Id == 2)?.Id, IsActive = true });
                _resources.Add(new Resource { Id = 3, Name = "Проектор Epson", Type = "Equipment", Description = "Переносной проектор для презентаций", ImagePath = DemoImages.Projector, ResponsiblePersonId = _users.FirstOrDefault(u => u.Id == 3)?.Id, IsActive = true });
                _resources.Add(new Resource { Id = 4, Name = "Фотограф (студия)", Type = "Photographer", Description = "Услуги корпоративного фотографа по бронированию слота", ImagePath = DemoImages.Photographer, ResponsiblePersonId = _users.FirstOrDefault(u => u.Id == 1)?.Id, IsActive = true });
                _resources.Add(new Resource { Id = 5, Name = "Ноутбук Dell", Type = "Equipment", Description = "Ноутбук для встреч и презентаций", ImagePath = DemoImages.Laptop, ResponsiblePersonId = _users.FirstOrDefault(u => u.Id == 2)?.Id, IsActive = true });
                _resources.Add(new Resource { Id = 6, Name = "Переговорка \"Зеленая\"", Type = "MeetingRoom", Description = "Уютная переговорка на 4 человека", ImagePath = DemoImages.RoomGreen, ResponsiblePersonId = _users.FirstOrDefault(u => u.Id == 4)?.Id, IsActive = true });
            }

            ApplyDefaultImages();
            Persist();
            WireReferences();
        }

        // Если база уже создана раньше, всё равно проставляем фото демо-ресурсам
        private void ApplyDefaultImages()
        {
            foreach (var r in _resources)
            {
                var current = r.ImagePath;
                var isRemote = !string.IsNullOrWhiteSpace(current) &&
                               (current.StartsWith("http://") || current.StartsWith("https://"));

                // локальный путь может быть указан, но файла уже нет
                var localMissing = !string.IsNullOrWhiteSpace(current) && !isRemote &&
                                   !File.Exists(Path.IsPathRooted(current)
                                       ? current
                                       : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, current));

                if (string.IsNullOrWhiteSpace(current) || localMissing)
                {
                    var image = DemoImages.ForResource(r);
                    if (!string.IsNullOrWhiteSpace(image))
                        r.ImagePath = image;
                }
            }
        }

        private int NextBookingId()
        {
            return _bookings.Any() ? _bookings.Max(b => b.Id) + 1 : 1;
        }

        private int NextResourceId()
        {
            return _resources.Any() ? _resources.Max(r => r.Id) + 1 : 1;
        }

        private int NextUserId()
        {
            return _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
        }
    }
}
