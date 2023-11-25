
using Core.Enumeration;

namespace Core.Model
{
    public class MailConfig
    {
        /// <summary>
        /// Máy chủ SMTP
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Cổng máy chủ SMTP
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Tên người gửi
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// Tên email gửi
        /// </summary>
        public string DisplayEmail { get; set; }
        /// <summary>
        /// Email người gửi
        /// </summary>
        public string SenderEmail { get; set; }
        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Mật khẩu
        /// </summary>
        public string PassWord { get; set; }
        /// <summary>
        /// Email Reply to
        /// </summary>
        public string ReplyTo { get; set; }
        public MailSecurityMethodEnum SecurityMethod { get; set; }

    }
}