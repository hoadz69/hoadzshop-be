using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Model.Platform
{
    public class LoginUserInfo : LoginUserInfoForClient
    {
        [Key]
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
        public Guid MisaId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string Avatar { get; set; }
        
        /// <summary>
        /// Các ứng dụng được phép sử dụng
        /// </summary>
        public string Applications { get; set; }
        
        /// <summary>
        /// Chuỗi mã phân quyền cho tất cả các app
        /// </summary>
        public string AccessTokenForApps { get; set; }
    }

    public class LoginUserInfoClientWithId : LoginUserInfoForClient
    {
        public Guid UserId { get; set; }
        public Guid TenantId { get; set; }
    }
    public class LoginUserInfoForClient
    {
        /// <summary>
        /// Tên tài khoản
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Họ và tên
        /// </summary>
        public string  FullName { get; set; }
        
        /// <summary>
        /// Tenant Code
        /// </summary>
        public string TenantCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MisaCode { get; set; }
        
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string Mobile { get; set; }
    }
}