using System;
using System.Collections.Generic;
using Core.Model.Platform;

namespace Core.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Lấy tên user hiện tại đang đăng nhập
        /// </summary>
        /// <returns></returns>
        string GetUserName();
        
        /// <summary>
        /// Lấy tenantID của user đang đăng nhập
        /// </summary>
        /// <returns></returns>
        Guid GetTenantId();
        
        /// <summary>
        /// Lấy TenantCode của user đang đăng nhập
        /// </summary>
        /// <returns></returns>
        string GetTenantCode();

        Guid GetUserId();
        Guid GetOrganizationUnitId();

        string GetFullName();
        string GetEmail();
        string GetMobile();
        string GetMisaCode();
        string GetApplications();
        string GetSessionId();
        string GetCulture();
        string GetAuthorization();
        
        /// <summary>
        /// Lấy toàn bộ thông tin của user đang đăng nhập
        /// </summary>
        /// <returns></returns>
        LoginUserInfo GetCurrentUser();
        
        /// <summary>
        /// Láy toàn bộ các thông tin user đang đăng nhập dùng cho client ( chỉ chứa thông tin public)
        /// </summary>
        /// <returns></returns>
        LoginUserInfoForClient GetCurrentUserInfoForClient();

        string GetAuthPayloadString();

        //List<SC_PermissionByApp> GetPermission();

        string GetToken();

        bool IsMobile();

        public string GetClientId();

        public string GetClientSecret();

        public int GetExp();
        
        








    }
}