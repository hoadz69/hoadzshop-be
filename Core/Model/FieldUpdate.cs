using System.Collections.Generic;

namespace Core.Model
{
    /// <summary>
    /// Thông tin phân trang grid
    /// </summary>
    public class FieldUpdate
    {
        /// <summary>
        /// Tên model
        /// </summary>
        public string ModelName { get; set; }
        
        /// <summary>
        /// Trường khoá chính
        /// </summary>
        public string FieldKey { get; set; }
        
        /// <summary>
        /// Giá trị trường khoá chính
        /// </summary>
        public int ValueKey { get; set; }
        
        /// <summary>
        /// Danh sách các cột update
        /// + Key: tên trường
        /// + Value: giá trị cập nhật
        /// </summary>
        public Dictionary<string,object> FieldNameAndValue { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public object DataModel { get; set; }
    }
}