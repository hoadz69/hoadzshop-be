using System;
using System.Collections.Generic;
//using AutoMapper.Configuration;
using Core.Interface;
using Microsoft.Extensions.Configuration;

namespace Core.Services
{
    internal class ConfigService : IConfigService
    {

        private readonly IConfigurationSection _appSetting;
        private readonly IConfigurationSection _connectionStrings;
        private readonly IConfigurationSection _apiUrls;
        private readonly IConfigurationSection _storage;
        private static Dictionary<int, string> _storageTypeDictionary;

        public ConfigService(IConfigurationSection configuration )
        {
            _appSetting = configuration.GetSection("AppSettings");
            _connectionStrings = configuration.GetSection("ConnectionStrings");
            _apiUrls = configuration.GetSection("ApiUrl");
            _storage = configuration.GetSection("Storage");
        }
        public string GetAppSetting(string key, string defaultValue = null)
        {
            var value = _appSetting[key];
            if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(defaultValue))
            {
                value = defaultValue;
            }

            return value;
        }

        public string GetConnectionString(string key)
        {
            return _connectionStrings[key];
        }

        public string GetApiUrl(string key, string defaultValue = null)
        {
            var value = _apiUrls[key];
            if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(defaultValue))
            {
                value = defaultValue;
            }

            return value;
        }

        public string GetStorageConfig(string key, string defaultValue = null)
        {
            var value = _storage[key];
            if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(defaultValue))
            {
                value = defaultValue;
            }

            return value;
        }

        public string GetStorageType(int storageKey)
        {
            // TODO
            if (_storageTypeDictionary == null)
            {
                var dic=new Dictionary<int,string>();
                foreach (var item in _storage.GetSection("StorageType").GetChildren())
                {
                    string itemName = item.Key;
                    foreach (var childItem in item.GetChildren())
                    {
                        string childItemName = childItem.Key;
                        dic.Add(Convert.ToInt32(childItem.Value),$"{itemName}/{childItemName}");
                    }
                }

                _storageTypeDictionary = dic;
            }

            if (!_storageTypeDictionary.ContainsKey(storageKey))
            {
                throw new Exception("Invalid StorageType");
            }
            return _storageTypeDictionary[storageKey];
        }
    }
}