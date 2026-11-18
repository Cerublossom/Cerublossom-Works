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
