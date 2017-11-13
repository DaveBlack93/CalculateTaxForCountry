using Model;
using System.Collections.Generic;

namespace DataAccessLayer
{
    public interface IDataAccessManager<T> where T : class
    {
        void StoreTaxesForCity(string filePath, T _remunerationDto);
        bool ExistFile(string path);
        IList<T> ReadJson(string filePath);
    }
}
