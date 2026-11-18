using System.Xml.Serialization;

namespace service.Models
{
    public class Resource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // ConferenceRoom, Equipment, Photographer, MeetingRoom, Other
        public string Description { get; set; }
        public int? ResponsiblePersonId { get; set; }

        [XmlIgnore]
        public User ResponsiblePerson { get; set; }

        public bool IsActive { get; set; } = true;

        public Resource() { }
    }
}
