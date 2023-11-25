using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Contant;
using Core.Model;
using Core.Ultitily;

namespace Core.Utility
{
    public class Common
    {
        private static Dictionary<string, string> _queryMySql = new Dictionary<string, string>();

        /// <summary>
        /// Lấy model type theo tên
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Type GetModelType(string nameSpace, string typeName)
        {
            Type type = null;
            type = Type.GetType($"{nameSpace}.{typeName}, {nameSpace}");
            if (type == null)
            {
                throw new ArgumentException($"Type [{typeName}] not found.");
            }

            return type;
        }
        /// <summary>
        /// Tạo ra instance baseModel
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static BaseModel CreateIntanceBaseModel(string nameSpace, string typeName)
        {
            Type type = null;
            type = Type.GetType($"{nameSpace}.{typeName}, {nameSpace}");
            if (type == null)
            {
                throw new ArgumentException($"Type [{typeName}] not found.");
            }

            var baseModel = (BaseModel) Activator.CreateInstance(type);
            return baseModel;
        }

        /// <summary>
        /// Lấy danh sách câu query trong file query.json
        /// </summary>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string QueryMySql(string path, string key)
        {
            if (!_queryMySql.ContainsKey(key))
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                if (!File.Exists(fullPath))
                {
                    return string.Empty;
                    // return "";
                }

                var data = File.ReadAllText(fullPath);
                _queryMySql = Converter.Deserialize<Dictionary<string, string>>(data);
                return _queryMySql.ContainsKey(key) ? _queryMySql[key] : string.Empty;
                // return _queryMySql.ContainsKey(key) ? _queryMySql[key] : "";
            }
            else
            {
                return _queryMySql[key];
            }
        }

        public static bool CheckAccessableDatabase(string currentAppCode, string needAccessAppCode)
        {
            List<string>appCodeCanAccessAllDatabase= new List<string>
            {
                AppCode.Auth,
                AppCode.Management,
                AppCode.Workflow
            };
            List<string> databaseAnyAppCodeCanAccess= new List<string>
            {
                AppCode.Notification,
                AppCode.Option
            };
            if (string.IsNullOrWhiteSpace(currentAppCode) ||
                appCodeCanAccessAllDatabase.Any(c => c.Equals(currentAppCode, StringComparison.OrdinalIgnoreCase)))
            {
                // API/Worker hiện tại được truy xuất vào bất kì database của ứng dụng khác
                return true;
            }else if(!string.IsNullOrWhiteSpace(needAccessAppCode) &&
            databaseAnyAppCodeCanAccess.Any(c => c.Equals(needAccessAppCode, StringComparison.OrdinalIgnoreCase)))

            {
                // Database cần truy xuất là ủa ứng dụng mà tất cả các ứng dụng khác được truy xuất
                return true;
            }
            else if (!string.IsNullOrWhiteSpace(currentAppCode) && !string.IsNullOrWhiteSpace(needAccessAppCode) &&
                     currentAppCode.Equals(needAccessAppCode,StringComparison.OrdinalIgnoreCase))
            {
                // APi truy xuất đúng database của ứng dụng
                return true;
            }

            return false;
        }
    }
}