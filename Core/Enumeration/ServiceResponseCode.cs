namespace Core.Enumeration
{
    public enum ServiceResponseCode
    {
        /// <summary>
        /// Thành công
        /// </summary>
        Succes=0,
        /// <summary>
        /// Không có quyền
        /// </summary>
        NotPermission=1,
        /// <summary>
        /// Có lỗi
        /// </summary>
        Error=2,
        /// <summary>
        /// Lỗi hệ thống 
        /// </summary>
        Exception=3
    }
}