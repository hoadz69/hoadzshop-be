namespace Core.Model
{
    public class ValidateResult
    {
        /// <summary>
        /// ID của bản ghi lỗi
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// Mã lỗi
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Nội dung lỗi
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Dữ liệu tuỳ biến mang thêm
        /// </summary>
        public object AdditionInfo { get; set; }
        
        /// <summary>
        /// Kiểu validate
        /// </summary>
        public Enumeration.ValidateType ValidateType { set; get; }
    }
}