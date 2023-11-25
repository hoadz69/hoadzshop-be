namespace Core.Model
{
    public class PagingRequest
    {
        /// <summary>
        /// Số bản ghi / 1 trang
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// Vị trí trang 
        /// </summary>
        public int PageIndex { get; set; }
        
        /// <summary>
        /// Các cột cần select
        /// </summary>
        public string Columns { get; set; }
        
        /// <summary>
        /// String filter của Grid
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// String Sort của Grid
        /// </summary>
        public string Sort { get; set; }
        
        /// <summary>
        /// Filter custom
        /// </summary>
        public string CustomFilter { get; set; }
        
        /// <summary>
        /// Có sử dụng store procedure hay không
        /// </summary>
        public bool UseSp  { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public QuickSearch QuickSearch { get; set; }
    }

    public class QuickSearch
    {
        public string SearchValue { get; set; }
        public string[] Columns { get; set; }
    }
}