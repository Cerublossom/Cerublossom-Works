using System;
using System.Xml.Serialization;

namespace service.Models
{
    [XmlRoot("User")]
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // Administrator, Employee
        public bool IsActive { get; set; } = true;

        [XmlIgnore]
        public string DisplayName => !string.IsNullOrWhiteSpace(FullName) ? FullName : Email;

        // Инициалы для аватара (например, "Иван Иванов" -> "ИИ")
        [XmlIgnore]
        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullName))
                    return "?";
                var parts = FullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1)
                    return parts[0].Length > 0 ? parts[0].Substring(0, 1).ToUpper() : "?";
                return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
            }
        }

        // Фото для карточки на экране "Люди". Ссылка на сервис портретов-заглушек,
        // номер картинки считается по Id, чтобы у разных людей были разные фото.
        [XmlIgnore]
        public string AvatarUrl
        {
            get
            {
                var key = Id != 0 ? Id : (FullName ?? Email ?? "user").GetHashCode();
                var number = (key & int.MaxValue) % 70 + 1;
                return "https://i.pravatar.cc/240?img=" + number;
            }
        }

        public User() { }

        public User(int id, string fullName, string email, string role = "Employee")
        {
            Id = id;
            FullName = fullName;
            Email = email;
            Role = role;
        }
    }
}
