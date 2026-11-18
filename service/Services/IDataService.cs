using System.Collections.Generic;
using service.Models;

namespace service.Services
{
    public interface IDataService
    {
        string DataFolder { get; }
        T Load<T>(string fileName) where T : class;
        void Save<T>(string fileName, T data);
    }
}
