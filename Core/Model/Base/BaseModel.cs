using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using DataType = Core.Enumeration.DataType;

namespace Core.Model
{
    public class BaseModel: ICloneable
    {
        public Guid TenantId { set; get; }
        public Guid UserId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        
        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime EditVersion { set; get;}

        public List<ModelDetailConfig> ModelDetailConfigs { get; set; }
        [NotMapped]
        public ModelState State { get; set; }

        public string OldData { get; set; }

        public string[] PassWarningCode { get; set; }
        /// <summary>
        /// Gán giá trị cho khoá chính
        /// </summary>
        /// <param name="value"></param>
        public void SetPrimaryKey(string value)
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            PropertyInfo propertyInfoKey = null;
            if (props != null)
            {
                propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                if (propertyInfoKey != null)
                {
                    if (propertyInfoKey.PropertyType == typeof(long))
                    {
                        propertyInfoKey.SetValue(this,long.Parse(value+""));
                    }
                    else if (propertyInfoKey.PropertyType == typeof(Int32))
                    {
                        propertyInfoKey.SetValue(this,int.Parse(value+""));
                    }
                    else if (propertyInfoKey.PropertyType == typeof(Guid))
                    {
                        propertyInfoKey.SetValue(this,Guid.Parse(value+""));
                    }
                    else
                    {
                        propertyInfoKey.SetValue(this,value);
                    }
                }
                
            }
            
        }
        /// <summary>
        /// Set giá trị cho khoá chính mặc định
        /// </summary>
        public void SetAutoPrimaryKey()
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            PropertyInfo propertyInfoKey = null;
            if (props != null)
            {
                propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                if (propertyInfoKey.PropertyType == typeof(long))
                {
                    //Long tự tăng
                }
                else if (propertyInfoKey.PropertyType == typeof(Int32))
                {
                    //Int tự tăng
                }
                else if (propertyInfoKey.PropertyType == typeof(Guid))
                {
                    propertyInfoKey.SetValue(this,Guid.NewGuid());
                }
                else
                {
                    //String có giá trị ban đầu
                }
                
            }
            
        }

        /// <summary>
        /// Lấy ra khoá chính
        /// </summary>
        /// <returns></returns>
        public object GetPrimaryKey()
        {
            return GetValueByAttribute(typeof(KeyAttribute));
        }

        /// <summary>
        /// Lấy giá trị khoá chính
        /// </summary>
        /// <returns></returns>
        public object GetPrimaryKeyValue()
        {
            return GetValueByAttribute(typeof(KeyAttribute));
        }
        
        /// <summary>
        /// Lấy ra giá trị của khoá chính
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public object GetPrimaryKeyValueFromRaw(string id)
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            PropertyInfo propertyInfoKey = null;
            if (props != null)
            {
                propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                Type type = propertyInfoKey.PropertyType;
                if ( type== typeof(long))
                {
                    return long.Parse(id);
                }
                else if (propertyInfoKey.PropertyType == typeof(Int32))
                {
                    return Int32.Parse(id);
                }
                
                else if (propertyInfoKey.PropertyType == typeof(Int64))
                {
                    return Int64.Parse(id);
                }
                else if (propertyInfoKey.PropertyType == typeof(Guid))
                {
                    return Guid.Parse(id);
                }
                
            }

            return null;

        }
        
        /// <summary>
        /// Lấy giá trị của khoá ngoại
        /// </summary>
        /// <returns></returns>
        public object GetForeignKeyValue()
        {
            return GetValueByAttribute(typeof(ForeignKeyAttribute));
        }

        /// <summary>
        /// Cập nhật giá trị cho khoá ngoại
        /// </summary>
        /// <param name="value"></param>
        public void SetForeignKeyValue(object value)
        {
            SetValueByAttribute(typeof(ForeignKeyAttribute), value);
        }

        /// <summary>
        /// Thiết lập giá trị theo khoá
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void SetValueByAttribute(Type type, object value)
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            PropertyInfo propertyInfo = null;
            if (props != null)
            {
                propertyInfo = props.SingleOrDefault(p => p.GetCustomAttribute(type,true) != null);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(this,value);
                }
            }
        }
        /// <summary>
        /// Lấy giá trị của 1 trường theo Attribute
        /// </summary>
        /// <param name="typeAttr"></param>
        /// <returns></returns>
        public object GetValueByAttribute(Type typeAttr)
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            PropertyInfo propertyInfo = null;
            if (props != null)
            {
                propertyInfo = props.SingleOrDefault(p => p.GetCustomAttribute(typeAttr,true) != null);
                if (propertyInfo != null)
                {
                    return propertyInfo.GetValue(this);
                }
            }

            return null;
        }
        public object GetValueFieldName(string fieldName)
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            PropertyInfo propertyInfo = null;
            if (props != null)
            {
                propertyInfo = props.SingleOrDefault(p => p.Name.Contains(fieldName,StringComparison.OrdinalIgnoreCase));
                if (propertyInfo != null)
                {
                    return propertyInfo.GetValue(this);
                }
            }

            return null;
        }
        
        /// <summary>
        /// lấy về tên khoá chính
        /// </summary>
        /// <returns></returns>
        public string GetPrimaryKeyFieldName()
        {
            return this.GetType().GetPrimaryKeyFieldName();
        }
        /// <summary>
        /// Lấy về tên khoá ngoại
        /// </summary>
        /// <returns></returns>
        public string GetForeignKeyFieldName()
        {
            return this.GetType().GetForeignKeyFieldName();
        }
        /// <summary>
        /// lấy về kiểu dữ liệu của khoá chính
        /// </summary>
        /// <returns></returns>
        public Type GetPrimaryKeyType()
        {
            return this.GetType().GetPrimaryKeyType();
        }
        /// <summary>
        /// check xem có chứa property không
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool ContainProperty(string propertyName)
        {
            return this.GetType().ContainProperty(propertyName);
        }

        /// <summary>
        /// lấy về tên bảng master của model hiện tại
        /// </summary>
        /// <returns></returns>
        public string GetMasterTableName()
        {
            return this.GetType().GetMasterTableName();
        }

        /// <summary>
        /// Lấy về tên bảng
        /// </summary>
        /// <returns></returns>
        public string GetTableNameOnly()
        {
            return this.GetType().GetTableNameOnly();
        }

        /// <summary>
        /// Lấy về có cột edit version hay không
        /// </summary>
        /// <returns></returns>
        public bool GetHasEditVersion()
        {
            return this.GetType().GetHasEditVersion();
        }

        /// <summary>
        /// Lấy về các cột check trùng không
        /// </summary>
        /// <returns></returns>
        public string GetFieldUnique()
        {
            return this.GetType().GetFieldUnique();
        }

        /// <summary>
        /// Lấy về tên bảng mapping trong database
        /// </summary>
        /// <returns></returns>
        public string GetViewOrTableName()
        {
            return this.GetType().GetViewOrTableName();
        }
        
        public string[] GetTableNameAndViewNameByType()
        {
            return this.GetType().GetTableNameAndViewNameByType();
        }
        /// <summary>
        /// Clone object
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object Clone()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// get set value cho 1 property (Indexer)
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="Exception"></exception>
        [IgnoreDataMember]
        [JsonIgnore]
        public object this[string propertyName]
        {
            get
            {
                PropertyInfo propertyInfo = this.GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    return propertyInfo.GetValue(this, null);
                }
                else
                {
                    throw new Exception(string.Format("<{0}> does not exist in object <{1}>", propertyName,
                        this.GetType().ToString()));
                }
            }
            set
            {
                PropertyInfo propertyInfo = this.GetType().GetProperty(propertyName);
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(this, value,null);
                }
                else
                {
                    throw new Exception(string.Format("<{0}> does not exist in object <{1}>", propertyName,
                        this.GetType().ToString()));
                }
            }
        }

        // public virtual string GetAuditingLogDescription(ResourceManager rm)
        // {
        //     string log = "";
        //     StringBuilder stringBuilder=new StringBuilder();
        //     PropertyInfo propertyInfo = null;
        //     string fieldChangTemplate = CoreResoucre.LogTemplate
        // }
        
        
        /// <summary>
        /// Lấy về giá trị của kiểu dữ liệu sau khi qua hàm format
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="value"></param>
        /// <param name="rm"></param>
        /// <returns></returns>
        private string ProcessValue(PropertyInfo propertyInfo, object value, ResourceManager rm)
        {
            string strValue = string.Empty;
            string fieldName = propertyInfo.Name;
            if (propertyInfo.PropertyType == typeof(string))
            {
                strValue = Convert.ToString(value);
            }
            else if (propertyInfo.PropertyType == typeof(decimal) || propertyInfo.PropertyType == typeof(double) ||
                     propertyInfo.PropertyType == typeof(decimal?) || propertyInfo.PropertyType == typeof(double?) ||
                     propertyInfo.PropertyType == typeof(Decimal) || propertyInfo.PropertyType == typeof(Decimal?) ||
                     propertyInfo.PropertyType == typeof(Double) || propertyInfo.PropertyType == typeof(Double?))
            {
                if (value != null)
                {
                    Enumeration.DataType dataType = Enumeration.DataType.CurrencyType;
                    if(fieldName.Contains("Amount")||fieldName.Contains("TotalAmount"))
                    {
                        dataType = Enumeration.DataType.CurrencyType;
                    }
                    else if (fieldName.Contains("Quantity"))
                    {
                        dataType = Enumeration.DataType.QuantityType;
                    }
                    else if (fieldName.Contains("Rate"))
                    {
                        dataType = Enumeration.DataType.ExChangeRateType;
                    }
                    else
                    {
                        dataType = Enumeration.DataType.NoFormat;
                    }

                    // strValue = NumberFormat.FormatNumber(Convert.ToDecimal((value.ToString(), dataType)));
                }
            }
            else if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?))
            {
                if (value != null)
                {
                    DateTime vDate = Convert.ToDateTime(value.ToString());
                    strValue = vDate.ToString("dd/MM/yyyy");
                }
            }
            else if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType == typeof(int?) ||
                     propertyInfo.PropertyType == typeof(Int32) || propertyInfo.PropertyType == typeof(Int32?) ||
                     propertyInfo.PropertyType == typeof(Int64) || propertyInfo.PropertyType == typeof(Int64?) ||
                     propertyInfo.PropertyType == typeof(Int16) || propertyInfo.PropertyType == typeof(Int16?))
            {
                if (value != null)
                {
                    strValue = Convert.ToString(value);
                }
            }
            else if (propertyInfo.PropertyType == typeof(bool) || propertyInfo.PropertyType == typeof(bool?))
            {
                if (value!=null)
                {
                    strValue = Convert.ToBoolean(value.ToString()) ? "true" : "false";
                }
                else
                {
                    strValue = "false";
                }
            }

            return strValue;
        }
    }
}