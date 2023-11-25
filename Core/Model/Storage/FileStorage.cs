using System;

namespace Core.Model.Storage
{
    public class FileStorage
    {
        /// <summary>
        /// tên file lưu trên storage
        /// </summary>
        public string FileId { get; set; }
        /// <summary>
        /// loại file
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// tên file gốc ban đầu
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Người tải lên (Id người tải)
        /// </summary>
        public Guid Uploader { get; set; }
        /// <summary>
        /// Thời gian tải lên
        /// </summary>
        public DateTime UploadTime { get; set; }
        /// <summary>
        /// Kích thước file
        /// </summary>
        public int FileSize { get; set; }
        /// <summary>
        /// Thời gian cuối cùng download xuống
        /// </summary>
        public DateTime LastDownloadTime { get; set; }
        /// <summary>
        /// Mã ứng dụng
        /// </summary>
        public string ApplicationCode { get; set; }
    }
}