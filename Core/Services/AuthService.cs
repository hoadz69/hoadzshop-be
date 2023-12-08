using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Core.Contant;
using Core.Interface;
using Core.Model.Platform;
using Core.Ultitily;
using Microsoft.AspNetCore.Http;

namespace Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly IConfigService _configService ;

        private string GetItemByName(string itemName)
        {
            return _httpContext?.HttpContext?.Items[itemName] + "";
        }
        private string GetHeaderByName(string headerName)
        {
            return _httpContext?.HttpContext?.Request.Headers[headerName] + "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetUserName()
        {
            return GetItemByName(Keys.UserName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Guid GetTenantId()
        {
            string tenantId = GetItemByName(Keys.TenantId);
            //if (string.IsNullOrEmpty(tenantId))
            //{
            //    return  new Guid(CommonConstant.INVALID_GUID);
            //}
            //else
            //{
            //    return Guid.Parse(tenantId);
            //}

            //fix cung tenant id
            return new Guid("24428d02-8e05-11ee-83f1-0242ac130003");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetTenantCode()
        {
            return GetItemByName(Keys.TenantCode);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Guid GetUserId()
        {
            string userId = GetItemByName(Keys.UserId);
            if (string.IsNullOrEmpty(userId))
            {
                return Guid.Empty;
            }
            else
            {
                return Guid.Parse(userId);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Guid GetOrganizationUnitId()
        {
            string organizationUnitId = GetItemByName(Keys.OrganizationUnitId);
            if (string.IsNullOrEmpty(organizationUnitId))
            {
                return  new Guid(CommonConstant.INVALID_GUID);
            }
            else
            {
                return Guid.Parse(organizationUnitId);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetFullName()
        {
            return GetItemByName(Keys.FullName);
        }

        public string GetEmail()
        {
            return GetItemByName(Keys.Email);
        }

        public string GetMobile()
        {
            return GetItemByName(Keys.Mobile);
        }

        public string GetMisaCode()
        {
            return GetItemByName(Keys.MisaCode);
        }

        public string GetApplications()
        {
            return GetItemByName(Keys.Applications);
        }

        public string GetSessionId()
        {
            return GetItemByName(Keys.SessionId);
        }

        public string GetCulture()
        {
            return GetItemByName(Keys.Culture);
        }

        public string GetAuthorization()
        {
            return GetItemByName(Keys.Authorization);
        }

        public LoginUserInfo GetCurrentUser()
        {
            if (_httpContext.HttpContext != null)
            {
                string jsonPayload = GetAuthPayloadString();
                return Converter.Deserialize<LoginUserInfo>(jsonPayload);
            }

            return null;
        }

        public LoginUserInfoForClient GetCurrentUserInfoForClient()
        {
            if (_httpContext.HttpContext != null)
            {
                string jsonPayload = GetAuthPayloadString();
                return Converter.Deserialize<LoginUserInfoForClient>(jsonPayload);
            }

            return null;
        }

        public string GetAuthPayloadString()
        {
            string authHeader = GetHeaderByName(Keys.Authorization);
            string token = authHeader.Split(new char[] { ' ' })[1];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);
            return jsonToken.Payload.SerializeToJson();
        }

        //public List<SC_PermissionByApp> GetPermission()
        //{
        //    var result=new List<SC_PermissionByApp>();
        //    var headerPermission = GetHeaderByName(Keys.Permission);
        //    if (!string.IsNullOrWhiteSpace(headerPermission))
        //    {
        //        var dic = Converter.Deserialize<Dictionary<string, object>>(Converter.DecryptAES(headerPermission));
        //        if (dic.ContainsKey(Keys.TenantId) && dic.ContainsKey(Keys.AppCode) && dic.ContainsKey(Keys.Expires) &&
        //            dic.ContainsKey(Keys.Permission))
        //        {
        //            string appCode = dic[Keys.AppCode].ToString();
        //            string currentAppCode = _configService.GetAppSetting(AppSettingsKey.ApplicationCode);
        //            if (appCode.Equals(currentAppCode, StringComparison.OrdinalIgnoreCase))
        //            {
        //                long expires = Convert.ToInt64(dic[Keys.Expires]);
        //                if ((new DateTime(expires)) >= DateTime.UtcNow)
        //                {
        //                    string permission = dic[Keys.Permission].ToString();
        //                    if (!string.IsNullOrWhiteSpace(permission))
        //                    {
        //                        result = Converter.Deserialize<List<SC_PermissionByApp>>(permission);
        //                    }
        //                }
        //            }
        //        }
                
        //    }
            
        //    return result;
        //}

        public string GetToken()
        {
            string authorization = GetItemByName(Keys.Authorization);
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                if (authorization.StartsWith(Keys.Bearer))
                {
                    authorization = authorization.Split(' ')[1];
                }
            }

            return authorization;
        }

        public bool IsMobile()
        {
            var deviceName=GetHeaderByName(Keys.DeviceName);
            return !string.IsNullOrWhiteSpace(deviceName);
        }

        public string GetClientId()
        {
            return GetHeaderByName(Keys.ClientId);
        }

        public string GetClientSecret()
        {
            return GetHeaderByName(Keys.ClientSecret);
        }

        public int GetExp()
        {
            return Convert.ToInt32(GetItemByName(Keys.Exp));
        }
    }
}