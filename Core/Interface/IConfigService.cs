namespace Core.Interface
{
    public interface IConfigService
    {
        string GetAppSetting(string key, string defaultValue = null);
        string GetConnectionString(string key);
        string GetApiUrl(string key, string defaultValue = null);
        string GetStorageConfig(string key, string defaultValue = null);
        string GetStorageType(int storageType);
    }
}