using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Core.Attribute;
using Core.Contant;

namespace Core
{
    /// <summary>
    /// Tạo ra các method cho type
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// kiểm tra chuỗi có tồn tại từ khoá hay không
        /// </summary>
        /// <param name="text"></param>
        /// <param name="vlaue"></param>
        /// <returns></returns>
        public static bool ContainsCaseInsensitive(this string text, string value)
        {
            StringComparison compare = StringComparison.CurrentCultureIgnoreCase;
            return text.IndexOf(value, compare) >= 0;
        }

        /// <summary>
        /// check Text Containt phone number
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool ContainPhoneNumber(this string text)
        {
            bool res = false;
            if (!string.IsNullOrWhiteSpace((text)))
            {
                Match match = Regex.Match(text, CommonConstant.RegexContainPhoneNumber);
                res = match.Success;
            }

            return res;
        }
        /// <summary>
        /// bỏ dấu chuỗi chứa tiếng việt
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToNonUnicode(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            StringBuilder result = new StringBuilder();
            string temp = text;
            temp = Regex.Replace(temp, "Đ|Đ|đ", "D");
            temp = Regex.Replace(temp, "đ|đ|Đ", "d");
            temp = Regex.Replace(temp, "-đ", " ");
            temp = temp.Normalize((NormalizationForm.FormKD));
            foreach (char s in temp)
            {
                if ((char.GetUnicodeCategory(s) != UnicodeCategory.NonSpacingMark) && !(char.IsPunctuation(s)) &&
                    !(char.IsSymbol(s)))
                {
                    result.Append(s);
                }
            }

            return result.ToString();

        }
        /// <summary>
        /// Chuyen datetime sang Iso Date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToIsoDate(this DateTime date)
        {
            return date.ToIsoDate();
        }
        /// <summary>
        /// thêm mới hoặc cập nhật key của dictionary
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate(this Dictionary<string, object> dic, string key, object value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key,value);
            }
        }
        /// <summary>
        /// lấy giá trị từ dictionary
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object Get(this Dictionary<string, object> dic, string key)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Thêm hoặc cập nhật dic2 vào dic ban đầu
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="dic2"></param>
        public static void AddOrUpdate(this Dictionary<string, object> dic, Dictionary<string,object> dic2)
        {
            if (dic2!=null&&dic2.Count>0)
            {
                foreach (var item in dic2)
                {
                    dic.AddOrUpdate(item.Key,item.Value);
                }
            }
        }

 
        /// <summary>
        /// cập nhật hoặc thêm giá trị cho key
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddOrUpdate(this Dictionary<string, string> dic, string key, string value)
        {
            if (dic.ContainsKey(key))
            {
                dic[key] = value;
            }
            else
            {
                dic.Add(key,value);
            }
        }
        /// <summary>
        /// lấy về giá trị thuộc tính của đối tượng
        /// </summary>
        /// <param name="objEntity">Đối tượng có thuộc tính cần lấy</param>
        /// <param name="propertyName">tên thuộc tính cần lần</param>
        /// <typeparam name="T">Kiểu dữ liệu trả về</typeparam>
        /// <returns></returns>
        public static T GetValue<T>(this object objEntity, string propertyName)
        {
            T value = default(T);
            if (objEntity != null && !string.IsNullOrEmpty(propertyName))
            {
                PropertyInfo propertyInfo = objEntity.GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    object objValue = propertyInfo.GetValue(objEntity);
                    if (objValue != null)
                    {
                        value = (T)objValue;
                    }
                }
            }

            return value;
        }
        /// <summary>
        /// hàm gán giá trị cho thuộc tính của đối tượng
        /// </summary>
        /// <param name="objEntity"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetValue(this object objEntity, string propertyName, object value)
        {
            PropertyInfo propertyInfo = objEntity.GetType().GetProperty(propertyName,BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo !=null)
            {
                Type type = propertyInfo.PropertyType;
                if ((!object.Equals(value, DBNull.Value)) && propertyInfo.CanWrite)
                {
                    if (value != null)
                    {
                        propertyInfo.SetValue(objEntity,Convert.ChangeType(value,Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType),null);
                    }
                }
                else
                {
                    propertyInfo.SetValue(objEntity,null,null);
                }
            }
        }

        #region "Extension for Type"

        /// <summary>
        /// lấy về tên cột của khoá chính
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetPrimaryKeyFieldName(this Type type)
        {
            return type.GetFieldName(typeof(KeyAttribute));
        }
        /// <summary>
        /// lấy về tên cột của khoá ngoạn
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetForeignKeyFieldName(this Type type)
        {
            return type.GetFieldName(typeof(ForeignKeyAttribute));
        }

        /// <summary>
        /// lấy ra tên cột chứa attribute
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attrType"></param>
        /// <returns></returns>
        public static string GetFieldName(this Type type, Type attrType)
        {
            string fieldName = string.Empty;
            PropertyInfo[] props = type.GetProperties();
            if (props != null)
            {
                var propertyInfo = props.SingleOrDefault(p => p.GetCustomAttribute(attrType, true) != null);
                if (propertyInfo != null)
                {
                    fieldName = propertyInfo.Name;
                }
            }

            return fieldName;
        }
        
        /// <summary>
        /// Lấy về kiểu dữ liệu của khoá chính
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetPrimaryKeyType(this Type type)
        {
            PropertyInfo[] props = type.GetProperties();
            PropertyInfo propertyInfo = null;
            if (props != null)
            {
                propertyInfo = props.SingleOrDefault(p => p.GetCustomAttribute(typeof(KeyAttribute), true) != null);
                if (propertyInfo != null)
                {
                    return propertyInfo.PropertyType;
                }
            }

            return typeof(object);
        }
        /// <summary>
        /// lấy ra Type của property
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Type GetPropertyType(this Type type, string propertyName)
        {
            PropertyInfo[] props = type.GetProperties();
            PropertyInfo propertyInfo = null;
            if (props != null)
            {
                propertyInfo = props.SingleOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase) != null);
            }
            if (propertyInfo != null)
            {
                return propertyInfo.PropertyType;
            }

            return typeof(object);
        }

        /// <summary>
        /// kiểm tra xem entity có chứa property hay không
        /// </summary>
        /// <param name="type"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool ContainProperty(this Type type, string property)
        {
            return (type.GetProperty(property) != null);
        }

        /// <summary>
        /// Lấy về tên bảng master, bảng hiện tại đang là bảng detail chứ khoá ngoại của bảng master
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetMasterTableName(this Type type)
        {
            string tableName = string.Empty;
            PropertyInfo[] props = type.GetProperties();
            if (props != null)
            {
                var propertyInfo = props.SingleOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute>(true) != null);
                if (propertyInfo != null)
                {
                    tableName = ((ForeignKeyAttribute) propertyInfo.GetCustomAttribute<ForeignKeyAttribute>(true)).Name;
                }
            }

            return tableName;
        }

        /// <summary>
        /// lấy ra tên bảng theo model
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableNameOnly(this Type type)
        {
            string tableName =
                ((ConfigTableAttribute) type.GetCustomAttributes(typeof(ConfigTableAttribute), false).FirstOrDefault())
                .TableName;
            return tableName;
        }
        /// <summary>
        /// lấy tên bảng mapping trong database
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetViewOrTableName(this Type type)
        {
            var configTable =
                ((ConfigTableAttribute) type.GetCustomAttributes(typeof(ConfigTableAttribute), false).FirstOrDefault());
            string tableName = configTable?.ViewName;
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = configTable?.TableName;
            }

            return tableName;
        }

        /// <summary>
        /// lấy ra tên bảng và tên view
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string[] GetTableNameAndViewNameByType(this Type type)
        {
            var names = new string[2] {string.Empty, string.Empty};
            var attributes = type.GetCustomAttributes(typeof(ConfigTableAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                names[0] = ((ConfigTableAttribute) attributes[0]).TableName;
                names[1] = ((ConfigTableAttribute) attributes[0]).ViewName;
                
            }

            return names;
        }

        public static bool GetHasEditVersion(this Type type)
        {
            return (((ConfigTableAttribute) type.GetCustomAttributes(typeof(ConfigTableAttribute), false).FirstOrDefault())?.HasEditVersion).GetValueOrDefault();
                ;
        }
        
        public static string GetFieldUnique(this Type type)
        {
            return ((ConfigTableAttribute) type.GetCustomAttributes(typeof(ConfigTableAttribute), false).FirstOrDefault())?.FieldUnique;
            ;
        }
        
        #endregion

        #region  URL

        /// <summary>
        /// encode url
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UrlEncode(this string value)
        {
            return WebUtility.UrlEncode(value);
        }

        /// <summary>
        /// Url Decode
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UrlDecode(this string value)
        {
            return WebUtility.UrlDecode(value);
        }
        #endregion

        #region Byte-Object converter

        /// <summary>
        /// Convert object sang byte array
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream,obj);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Convert byte array to object
        /// </summary>
        /// <param name="bytes"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ToObject<T>(this byte[] bytes)
        {
            if (bytes == null)
            {
                return default(T);
            }
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream=new MemoryStream())
            {
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        #endregion

        
        
    }
}