namespace Core.Contant
{
    public class DatabaseConstant
    {
        /// <summary>
        /// Số bản ghi tối đa có thể trả về
        /// </summary>
        public const int MaxReturnRecord = 5000;
        
        /// <summary>
        /// Số bản ghi tối đa có thể trả về
        /// </summary>
        public const int MaxPageIndex = 500;
        
        /// <summary>
        /// cấu trúc proc để lấy paging
        /// </summary>
        public const string SQLPagingSpTemplate = "Proc_{0}_GetPaging";
        
        /// <summary>
        /// tên schema mặc định
        /// </summary>
        public const string DefaultSchemaName = "dbo";
        /// <summary>
        /// tên model mặc định
        /// </summary>
        public const string DefaultModelNamespace = "Core.Model.{0},Core.Model";
        
        public const string Proc_Insert = "Proc_{0}_Insert";
        public const string Proc_Update = "Proc_{0}_Update";
        public const string Proc_Delete = "Proc_{0}_Delete";
        public const string Proc_GetById = "Proc_{0}_GetById";

        public const string EditVersion = "EditVersion";
        public const string EditVersionConvert = "EditVersionConvert";

        public const string CreatedDate = "CreatedDate";
        public const string CreatedBy = "CreatedBy";
        public const string ModifiedDate = "ModifiedDate";
        public const string ModifiedBy = "ModifiedBy";

        public class ConnectionString
        {
            public const string Management = "Management";
            public const string Auth = "Auth";
            public const string WesignAttestation = "WesignAttestation";
            public const string Notification = "Notification";
        }
    }
}