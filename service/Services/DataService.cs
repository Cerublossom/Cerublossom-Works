using System;
using System.IO;
using System.Xml.Serialization;

namespace service.Services
{
    public class DataService
    {
        public string DataFolder { get; }

        public DataService(string dataFolder = null)
        {
            DataFolder = dataFolder ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);
        }

        public T Load<T>(string fileName) where T : class
        {
            var path = Path.Combine(DataFolder, fileName);
            if (!File.Exists(path))
                return null;

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                using (var stream = File.OpenRead(path))
                {
                    return serializer.Deserialize(stream) as T;
                }
            }
            catch
            {
                return null;
            }
        }

        public void Save<T>(string fileName, T data)
        {
            var path = Path.Combine(DataFolder, fileName);
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = File.Create(path))
            {
                serializer.Serialize(stream, data);
            }
        }
    }
}
