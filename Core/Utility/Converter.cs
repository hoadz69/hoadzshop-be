using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Core.Ultitily
{
    public class Converter
    {
        #region JSON

        public static readonly DateFormatHandling JsonDateFormatHandling = DateFormatHandling.IsoDateFormat;
        public static readonly DateTimeZoneHandling JsonDateTimeZoneHandling = DateTimeZoneHandling.Local;
        public static readonly string JsonDateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK";
        public static readonly NullValueHandling JsonNullValueHandling = NullValueHandling.Include;
        public static readonly ReferenceLoopHandling JsonReferenceLooopHandling = ReferenceLoopHandling.Ignore;

        /// <summary>
        /// Custom lại format
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerSettings GetJsonSerializeSetting()
        {
            return new JsonSerializerSettings()
            {
                DateFormatHandling = JsonDateFormatHandling,
                DateTimeZoneHandling =JsonDateTimeZoneHandling,
                DateFormatString = JsonDateFormatString,
                NullValueHandling = JsonNullValueHandling,
                ReferenceLoopHandling = JsonReferenceLooopHandling
            };
        }
        /// <summary>
        /// Serialize object
        /// </summary>
        /// <param name="obj">Đối tượng cần serialize</param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, GetJsonSerializeSetting());
        }
        /// <summary>
        /// Deserailize object
        /// </summary>
        /// <param name="json">Chuỗi cần convert về kiểu T</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, GetJsonSerializeSetting());
        }
        
        /// <summary>
        /// Deserizalize object theo type
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object DeserializeObject(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, GetJsonSerializeSetting());
        }
        #endregion

        #region Base64

        public static string Base64Encode(string clearText)
        {
            if (string.IsNullOrWhiteSpace(clearText))
            {
                return string.Empty;
            }

            return Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(clearText));
        }

        public static string Base64Decode(string base64Text)
        {
            if (string.IsNullOrWhiteSpace(base64Text))
            {
                return string.Empty;
            }

            return UTF8Encoding.UTF8.GetString(Convert.FromBase64String(base64Text));
        }

        #endregion

        #region SHA

        /// <summary>
        /// Hash SHA
        /// </summary>
        /// <param name="input">String cần hash</param>
        /// <returns></returns>
        public static string SHA256Hash(string input)
        {
            HashAlgorithm hashAlgorithm= new SHA256CryptoServiceProvider();
            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);
            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);
            return Convert.ToBase64String(byteHash);
        }

        #endregion

        #region HTML Encode/Decode
        
        /// <summary>
        /// Encode Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlEncode(string url)
        {
            return WebUtility.UrlDecode(url);
        }
        
        /// <summary>
        /// Decode url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlDecode(string url)
        {
            return WebUtility.UrlDecode(url);
        }

        /// <summary>
        /// EnCode Html bảo mật
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string TextEncode(string content)
        {
            return WebUtility.HtmlEncode(content);
        }
        
        /// <summary>
        /// Decode html bảo mật
        /// </summary>
        /// <param name="textEncoded"></param>
        /// <returns></returns>
        public static string TextDecode(string textEncoded)
        {
            return WebUtility.HtmlDecode(textEncoded);
        }
        
        #endregion

        #region JWT
        
        /// <summary>
        /// Decrypt JWT payload
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns></returns>
        public static string JwtDecode(string jwt)
        {
            var handler=new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(jwt);
            var value = Serialize(jsonToken.Payload);
            return value;
        }
        
        #endregion

        #region Cookie
        
        /// <summary>
        /// Thêm cookie vào response của request
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cookieName">Tên cookie</param>
        /// <param name="cookieValue">giá trị của cookie</param>
        /// <param name="domain"> bắt buộc phải truyền _configService.GetAppSetting(AppSettingsKey.DomainUrl)</param>
        /// <param name="path">Mặc định không truyền là "/"</param>
        /// <param name="httpOnly">mặc định bằng true, không cho javascript truy xuất</param>
        /// <param name="expires">Thời gian hiệu lực cookie, mặc định k truyền là session cookie</param>
        public static void AddCookie(HttpResponse response, string cookieName,
            string cookieValue, string domain = null, string path = null, bool httpOnly = true,
            DateTime? expires = null)
        {
            var option= new CookieOptions()
            {
                HttpOnly = httpOnly
            };
            if (!string.IsNullOrWhiteSpace(domain))
            {
                option.Domain = domain;
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                option.Path = path;
            }

            if (expires.HasValue)
            {
                option.Expires = expires.Value;
            }
            response.Cookies.Append(cookieName,cookieValue,option);
        }

        /// <summary>
        /// Xoá cookie
        /// </summary>
        /// <param name="response"></param>
        /// <param name="cookieName"></param>
        /// <param name="domain">bắt buộc phải truyền _configService.GetAppSetting(AppSettingsKey.DomainUrl)</param>
        /// <param name="path">Mặc định không truyền là "/"</param>
        public static void DeleteCookie(Microsoft.AspNetCore.Http.HttpResponse response, string cookieName,
            string domain = null, string path = null)
        {
            var option = new Microsoft.AspNetCore.Http.CookieOptions();
            if (!string.IsNullOrWhiteSpace(domain))
            {
                option.Domain = domain;
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                option.Path = path;
            }
            response.Cookies.Delete(cookieName,option);
        }
        #endregion

        #region Encrypt/Decrypt

        private const string ENCRYPT_PASSWORD = "5000459276104ddf";
        private const string ENCRYPT_SALT = "tdlam";

        /// <summary>
        /// Mã hoá AES
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string EncryptAES(string plainText)
        {
            return EncryptAES(plainText, ENCRYPT_PASSWORD, ENCRYPT_SALT);
        }

        public static string EncryptAES(string plainText, string password, string salt)
        {
            try
            {
                using (Aes aes = new AesManaged())
                {
                    var deriveBytes= new Rfc2898DeriveBytes(password,Encoding.UTF8.GetBytes(salt));
                    aes.Key = deriveBytes.GetBytes(128 / 8);
                    aes.IV = aes.Key;
                    using (MemoryStream ecryptStream = new MemoryStream())
                    {
                        using (CryptoStream ecrypt =new CryptoStream(ecryptStream,aes.CreateEncryptor(),CryptoStreamMode.Write))
                        {
                            byte[] utfD1 = UTF8Encoding.UTF8.GetBytes(plainText);
                            ecrypt.Write(utfD1,0,utfD1.Length);
                            ecrypt.FlushFinalBlock();
                        }

                        return Convert.ToBase64String(ecryptStream.ToArray());
                    }
                    
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Giải mã AES
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string DecryptAES(string plainText)
        {
            return DecryptAES(plainText, ENCRYPT_PASSWORD, ENCRYPT_SALT);
        }
        public static string DecryptAES(string encryptText, string password, string salt)
        {
            try
            {
                using (Aes aes = new AesManaged())
                {
                    var deriveBytes= new Rfc2898DeriveBytes(password,Encoding.UTF8.GetBytes(salt));
                    aes.Key = deriveBytes.GetBytes(128 / 8);
                    aes.IV = aes.Key;
                    using (MemoryStream decryptStream = new MemoryStream())
                    {
                        using (CryptoStream decrypt =new CryptoStream(decryptStream,aes.CreateDecryptor(),CryptoStreamMode.Write))
                        {
                            byte[] encryptData = Convert.FromBase64String(encryptText);
                            decrypt.Write(encryptData,0,encryptData.Length);
                            decrypt.Flush();
                        }

                        byte[] decryptData = decryptStream.ToArray();
                        return UTF8Encoding.UTF8.GetString(decryptData, 0, decryptData.Length);
                    }
                    
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static string GenerateSecretKey()
        {
            var buf = new byte[32];
            (new RNGCryptoServiceProvider()).GetBytes(buf);
            return Convert.ToBase64String(buf);
        }
        #endregion

        #region  Convert Database Parameter
        /// <summary>
        /// Covert parameter từ data model hay 2 datamodel sang object truyền vào store
        /// </summary>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        /// <returns></returns>
        public static object ConvertDatabaseParam(object object1, object object2 = null)
        {
            var result = new Dictionary<string, object>();
            AppendPropertyToDictionary(object1, ref result);
            AppendPropertyToDictionary(object2, ref result);
            return result;
        }

        /// <summary>
        /// Convert param để truyền vào store
        /// </summary>
        /// <param name="object1"></param>
        /// <param name="result"></param>
        private static void AppendPropertyToDictionary(object object1, ref Dictionary<string, object> result)
        {
            if (object1 != null)
            {
                if (object1.GetType() == typeof(Dictionary<string, object>))
                {
                    foreach (var item in (Dictionary<string,object>)object1)
                    {
                        if (!item.Value.GetType().IsGenericType||item.Value.GetType().GetGenericTypeDefinition()!=typeof(ICollection<>)||item.Value.GetType().GetGenericTypeDefinition()!=typeof(List<>))
                        {
                            result.AddOrUpdate((item.Key.StartsWith("v_")?item.Key: ("v_"+item.Key)),item.Value);
                        }
                    }
                }
                else
                {
                    foreach (PropertyInfo propertyInfo in object1.GetType().GetProperties().Where(x=>x.CanRead&&x.GetIndexParameters().Length==0))
                    {
                        if (!propertyInfo.PropertyType.IsGenericType||propertyInfo.PropertyType.GetGenericTypeDefinition()!=typeof(ICollection<>)||propertyInfo.PropertyType.GetGenericTypeDefinition()!=typeof(List<>))
                        {
                            result.AddOrUpdate((propertyInfo.Name.StartsWith("v_")?propertyInfo.Name: ("v_"+propertyInfo.Name)),propertyInfo.GetValue(object1));
                        }
                    }
                }
            }
        }

        #endregion

        #region Convert List<object>To Datatable

        public static DataTable ConvertListObjectToDataTable(List<Dictionary<string, object>> data,
            bool isConvertToStringValue = false, string culture = "vi-VN",
            Dictionary<string, string> customFormats = null)
        {
            var dataTable = new DataTable();
            if (data != null&&data.Count>0)
            {
                foreach (var dataItem in data)
                {
                    foreach (var item in dataItem)
                    {
                        if (!dataTable.Columns.Contains(item.Key) && item.Value != null)
                        {
                            var dataColumn = new DataColumn()
                            {
                                ColumnName = item.Key,
                                DataType = (isConvertToStringValue ? typeof(string): item.Value.GetType())
                            };
                            if (isConvertToStringValue)
                            {
                                dataColumn.DefaultValue = string.Empty;
                            }

                            dataTable.Columns.Add(dataColumn);

                        }
                    }

                    var dr = dataTable.NewRow();
                    foreach (DataColumn dataColumn in dataTable.Columns)
                    {
                        var columnName = dataColumn.ColumnName;
                        if (dataItem[columnName] != null)
                        {
                            dr[columnName] = (isConvertToStringValue
                                ? ConvertObjectValueToStringValue(dataItem[columnName], columnName,
                                    dataItem[columnName].GetType(), culture: culture,
                                    customFormat: ((customFormats != null && customFormats.ContainsKey(columnName))
                                        ? customFormats[columnName]
                                        : string.Empty)):dataItem[columnName]);
                        }
                        else
                        {
                            if (isConvertToStringValue)
                            {
                                dr[columnName] = string.Empty;
                            }
                            else
                            {
                                dr[columnName] = DBNull.Value;
                            }
                        }
                    }

                    dataTable.Rows.Add(dr);
                }
            }

            return dataTable;
        }
        public static DataTable ConvertListObjectToDataTable(List<object> data,
            bool isConvertToStringValue = false, string culture = "vi-VN",
            Dictionary<string, string> customFormats = null)
        {
            var dataTable = new DataTable();
            if (data != null&&data.Count>0)
            {
                var dicProperty= new Dictionary<PropertyInfo,Type>();
                var properties = data[0].GetType().GetProperties().Where(c => c.CanRead);
                foreach (var property in properties)
                {
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var dc = new DataColumn()
                    {
                        ColumnName = property.Name,
                        DataType = (isConvertToStringValue ? typeof(string): property.PropertyType)
                    };
                    dataTable.Columns.Add(dc);

                }
                foreach (var item in data)
                {
                    var dr = dataTable.NewRow();
                    foreach (var property in dicProperty)
                    {
                        var value = property.Key.GetValue(item, null);
                        dr[property.Key.Name] = (value != null
                            ? (isConvertToStringValue
                                ? ConvertObjectValueToStringValue(value, property.Key.Name, property.Value,
                                    culture: culture,
                                    customFormat: ((customFormats != null &&
                                                    customFormats.ContainsKey(property.Key.Name))
                                        ? customFormats[property.Key.Name]
                                        : string.Empty))
                                : value)
                            : DBNull.Value);
                    }
                }
            }

            return dataTable;
        }

        private static object ConvertObjectValueToStringValue(object value, object propertyName, Type dataType, string culture="vi-VN", string customFormat=null)
        {
            if (value != null)
            {
                if (string.IsNullOrWhiteSpace(culture))
                {
                    culture = "vi-VN";
                }

                switch (Type.GetTypeCode(dataType))
                {
                    case TypeCode.Boolean:
                        return ((bool) value ? "Có" : "Không");
                    case TypeCode.SByte:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((sbyte)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.Byte:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((byte)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.Int16:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((short)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.Int32:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((int)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.UInt16:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((ushort)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.UInt32:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((uint)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.Int64:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((long)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.UInt64:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((ulong)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.Single:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((Single)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.Double:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((double)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.Decimal:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((decimal)value).ToString(customFormat, new CultureInfo(culture));
                        }

                        break;
                    case TypeCode.DateTime:
                        if (!string.IsNullOrWhiteSpace(customFormat))
                        {
                            return ((DateTime)value).ToString(customFormat, new CultureInfo(culture));
                        }
                        else
                        {
                            return ((DateTime) value).ToString((new CultureInfo(culture)).DateTimeFormat
                                .ShortDatePattern);
                        }
                    
                    
                }

                return value.ToString();
            }

            return null;
        }
        
        /// <summary>
        /// Mapping object từ source sang destination
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns></returns>
        public static TDestination MapObject<TSource, TDestination>(TSource source)
        {
            var config = new MapperConfiguration(cfg => { cfg.CreateMap<TSource, TDestination>(); });
            IMapper mapper = config.CreateMapper();
            var result = mapper.Map<TSource, TDestination>(source);
            return result;
        }
        #endregion

        #region ConvertName
        
        /// <summary>
        /// Build fullname từ firstName and lastName
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        public static string BuildFullNameFromParts(string firstName, string lastName)
        {
            var fullName = !string.IsNullOrWhiteSpace(firstName) ? firstName.Trim() : string.Empty;
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                fullName = !string.IsNullOrWhiteSpace(fullName) ? $"{fullName} {lastName.Trim()}" : lastName.Trim();
            }

            return fullName;
        }

        public static string[] SplitFullNameToParts(string fullName)
        {
            var parts = new string[2] {string.Empty, string.Empty};
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var lastIndexSpace = fullName.Trim().LastIndexOf(" ");
                if (lastIndexSpace > 0)
                {
                    parts[0] = fullName.Trim().Substring(0, lastIndexSpace).Trim();
                    parts[1] = fullName.Trim().Substring(lastIndexSpace + 1);
                }
                else
                {
                    parts[1] = fullName.Trim();
                }
                
            }

            return parts;
        }

        public static void SplitFullNameToParts(string fullName, ref string firstName, ref string lastName)
        {
            var parts = SplitFullNameToParts(fullName);
            firstName = parts[0];
            lastName = parts[1];
        }

        public static string RemoveVietNameseSign(string text, string replaceSpaceBy = " ")
        {
            var result = text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var originalChar = new string[]
                {
                    "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                    "ÉÈẸẺẼÊẾỀỆỂỄ",
                    "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                    "ÚÙỤỦŨƯỨỪỰỬỮ",
                    "ÍÌỊỈĨ",
                    "Đ",
                    "ÝỲỴỶỸ"
                };
                var replaceChar = new string[] {"A", "E", "O", "U", "I", "D", "Y"};
                for (int i = 0; i < originalChar.Length; i++)
                {
                    var reg=new Regex("["+originalChar[i]+"]");
                    result = reg.Replace(result, replaceChar[i]);
                    reg=new Regex("["+originalChar[i].ToLower()+"]");
                    result = reg.Replace(result, replaceChar[i]);
                    
                }
                if (!string.IsNullOrWhiteSpace(replaceSpaceBy) && !replaceChar.Equals(" "))
                {
                    result = result.Replace(" ", replaceSpaceBy);
                }
                
            }
            return result;

        }

        public static string ConvertToShortName(string fullName)
        {
            string shortName = string.Empty;

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var nameParts = fullName.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 2)
                {
                    shortName = $"{nameParts.First().Substring(0, 1)}{nameParts.Last().Substring(0, 1)}".ToUpper();
                }
                else
                {
                    shortName = nameParts[0].ToUpper();
                }
                var originalChar = new string[]
                {
                    "ÁÀẠẢÃ","ẤẦẬẨẪ","ẮẰẶẲẴ",
                    "ÉÈẸẺẼ","ÊẾỀỆỂỄ",
                    "ÓÒỌỎÕ","ÔỐỒỘỔỖ","ƠỚỜỢỞỠ",
                    "ÚÙỤỦŨ","ƯỨỪỰỬỮ",
                    "ÍÌỊỈĨ",
                    "ÝỲỴỶỸ"
                };
                var replaceChar = new string[] {"A","Â", "Ă","E","Ê", "O","Ô","Ơ", "U","Ư", "I", "Y"};
                for (int i = 0; i < originalChar.Length; i++)
                {
                    var reg=new Regex("["+originalChar[i]+"]");
                    shortName = reg.Replace(shortName, replaceChar[i]);
                }
            }
            return shortName;
        }
        #endregion
        
    }
}