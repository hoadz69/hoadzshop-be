namespace Core.Attribute
{
    public class ConfigTableAttribute : System.Attribute
    {
        //tên bảng
        public string TableName { get; set; }
        //tên view
        public string ViewName { get; set; }
        //có cột edit version không
        public bool HasEditVersion { get; set; }
        //danh sách cột unique cách nhau bởi dấu ;
        public string FieldUnique { get; set; }
        /// <summary>
        /// Khởi tạo
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="viewName"></param>
        /// <param name="hasEditVersion"></param>
        /// <param name="fieldUnique"></param>
        public ConfigTableAttribute(string tableName, string viewName="",bool hasEditVersion=false,string fieldUnique="")
        {
            TableName = tableName;
            ViewName = viewName;
            HasEditVersion = hasEditVersion;
            FieldUnique = fieldUnique;
        }
        
    }
}