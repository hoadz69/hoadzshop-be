namespace Core.Model
{
    public class PagingResponse
    {
        /// <summary>
        /// Dữ liệu phân trang
        /// </summary>
        public object PageData { get; set; }
        
        public int Total { get; set; }

        public PagingResponse()
        {
            
        }

        public PagingResponse(object pageData, int total, bool isCompressData = false)
        {
            PageData = pageData;
            Total = total;
        }
    }
}