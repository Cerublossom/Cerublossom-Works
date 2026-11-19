using service.Models;

namespace service.Services
{
    // Демонстрационные фото ресурсов.
    // Картинки взяты с Wikimedia Commons (свободные лицензии), 960px по ширине.
    // Для работы нужен интернет; можно заменить на свои файлы в папке Images.
    public static class DemoImages
    {
        public const string RoomA =
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/2/26/Conference_room_table_%28Unsplash%29.jpg/960px-Conference_room_table_%28Unsplash%29.jpg";

        public const string RoomB =
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/2/29/Cramped_meeting_room_%28Unsplash%29.jpg/960px-Cramped_meeting_room_%28Unsplash%29.jpg";

        public const string Projector =
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/1/14/InFocus_IN34.jpg/960px-InFocus_IN34.jpg";

        public const string Photographer =
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/f/fc/Photo_studio_set_up_at_museum_to_photograph_objects-artifacts-01.jpg/960px-Photo_studio_set_up_at_museum_to_photograph_objects-artifacts-01.jpg";

        public const string Laptop =
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/3/3e/Books%2C_pencils%2C_laptop%2C_and_iphone_on_a_desk_%28Unsplash%29.jpg/960px-Books%2C_pencils%2C_laptop%2C_and_iphone_on_a_desk_%28Unsplash%29.jpg";

        public const string RoomGreen =
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/a/ae/Conference_Room_at_the_Guy_Bradley_Visitor_Center_%2853412700646%29.jpg/960px-Conference_Room_at_the_Guy_Bradley_Visitor_Center_%2853412700646%29.jpg";

        // Подбираем фото по смыслу названия, а если не вышло — по типу ресурса.
        // Так работает и со старыми базами, где ресурсы названы иначе.
        public static string ForResource(Resource resource)
        {
            if (resource == null)
                return null;

            var name = (resource.Name ?? string.Empty).ToLowerInvariant();

            if (name.Contains("конференц") || name.Contains("переговор"))
            {
                // второй зал / зал "Б" получает другое фото
                if (name.Contains("2") || name.Contains("б") || name.Contains("b"))
                    return RoomB;
                return RoomA;
            }

            if (name.Contains("проектор") || name.Contains("projector"))
                return Projector;

            if (name.Contains("ноутбук") || name.Contains("laptop") || name.Contains("dell"))
                return Laptop;

            if (name.Contains("фотограф") || name.Contains("студи") || name.Contains("photo"))
                return Photographer;

            switch (resource.Type)
            {
                case "ConferenceRoom": return RoomA;
                case "MeetingRoom": return RoomGreen;
                case "Photographer": return Photographer;
                case "Equipment": return Laptop;
                default: return null;
            }
        }
    }
}
