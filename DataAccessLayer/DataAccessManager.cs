using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;
using Model;

namespace DataAccessLayer
{
    public class DataAccessManager<T> : IDataAccessManager<T> where T : class
    {
        public DataAccessManager()
        {   }

        #region Public Method

        public void StoreTaxesForCity(string filePath, T _remunerationDto)
        {
            if (_remunerationDto == null)
                throw new ArgumentNullException();

            IList<T> jsonFile = ReadJson(filePath);
            jsonFile.Add(_remunerationDto);
            var remunerationDtoToJson = JsonConvert.SerializeObject(jsonFile);
            saveToFile(remunerationDtoToJson);
        }

        public bool ExistFile(string path)
        {
            if (path == null)
                throw new ArgumentNullException();

            return File.Exists(path);
        }

        public IList<T> ReadJson(string filePath)
        {
            IList<T> items = new List<T>();

            bool fileExist = ExistFile(filePath);

            if (fileExist)
            {
                using (StreamReader streamReader = new StreamReader(filePath))
                {
                    string jsonFile = streamReader.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<T>>(jsonFile);
                }
            }

            return items;
        }

        #endregion

        #region Private Method

        private void saveToFile(string remunerationDtoToJson)
        {
            using (StreamWriter file = File.CreateText(Constants.FILEPATH))
            {
                file.Write(remunerationDtoToJson);
            }
        }

        #endregion
    }
}
