namespace Core.Model
{
    public class ModelDetailConfig
    {
        /// <summary>
        /// tên bảng detail
        /// </summary>
        public string DetailTableName { get; set; }
        /// <summary>
        /// Tên khoá ngoại
        /// </summary>
        public string ForeignKeyName { get; set; }
        /// <summary>
        /// List kiểu dữ liệu của detail trên master model
        /// </summary>
        public string PropertyOnMasterModel { get; set; }
        /// <summary>
        /// Có xoá detail trước khi xoá master hay không
        /// </summary>
        public bool CascadeOnDeleteMasterModel { get; set; }

        public ModelDetailConfig(string detailTableName, string foreignKeyName, string propertyOnMasterModel, bool cascadeOnDeleteMasterModel)
        {
            DetailTableName = detailTableName;
            ForeignKeyName = foreignKeyName;
            PropertyOnMasterModel = propertyOnMasterModel;
            CascadeOnDeleteMasterModel = cascadeOnDeleteMasterModel;
        }
    }
}