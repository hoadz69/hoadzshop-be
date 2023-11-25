using System;
using System.Threading.Tasks;
using Core.Contant;
using Core.Database;
using Core.Interface;
using Core.Model.Platform;
using Core.Services;
using Core.Ultitily;
using Microsoft.AspNetCore.Http;

namespace Core.BL
{
    internal class SessionBL : ISessionBL
    {
        private readonly ICacheService _cacheService;
        private readonly IDatabaseService _databaseService;

        public SessionBL(ICacheService cacheService, IDatabaseService databaseService)
        {
            _cacheService = cacheService;
            _databaseService = databaseService;
        }

        public async Task<string> GetAccessTokenBySessionId(string sessionId)
        {
            var accessTokenCacheName = string.Format(CacheKey.CacheAccessToken, sessionId);
            var accessToken = await _cacheService.Get<string>(accessTokenCacheName, isAppendAppCodeToKey: false);
#if DEBUG
            accessToken = null;

#endif
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                accessToken = await _databaseService.ExecuteScalarUsingStoredProcedure<string>(Guid.Empty, AppCode.Auth,
                    "Proc_Session_GetAccessTokenBySessionID", new {v_SessionID = sessionId});
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    await SetSessionForCache(sessionId, accessToken);
                }

                return accessToken ?? string.Empty;
            }
            else
            {
                return accessToken;
            }
        }

        public async Task<SC_AccessTokenByApp> GetAppInfoBySessionId(string sessionId, string appCode)
        {
            var appsToken = await _databaseService.ExecuteScalarUsingStoredProcedure<string>(Guid.Empty, AppCode.Auth,
                "Proc_Session_GetAppsTokenBySessionID", new {v_SessionID = sessionId, v_AppCode = appCode});
            if (!string.IsNullOrWhiteSpace(appCode))
            {
                return Converter.Deserialize<SC_AccessTokenByApp>(appsToken);
            }

            return null;
        }

        public async Task<string> GetMisaIdTokenBySessionId(string sessionId)
        {
            var misaIDToken = await _databaseService.ExecuteScalarUsingStoredProcedure<string>(Guid.Empty, AppCode.Auth,
                "Proc_Session_UpdateMISAIDTokenBySessionID", new {v_SessionID = sessionId});
            return misaIDToken;
        }

        public async Task UpdateMisaIdTokenBySessionId(string sessionId, string misaIdToken)
        {
            var param = new
            {
                v_SessionID = sessionId,
                v_MISAIDToken = misaIdToken
            };
            var appsToken = await _databaseService.ExecuteScalarUsingStoredProcedure<string>(Guid.Empty, AppCode.Auth,
                "Proc_Session_UpdateMISAIDTokenBySessionID", param);
        }

        public async Task AddNewSession(string sessionId, bool isNewSession, Guid userId, Guid tenantId, long ticks,
            string accessToken,
            string appsToken, Guid misaId, string misaIdToken)
        {
            bool renewSession = false;
#if DEBUG
            renewSession = true;
#endif
            var param = new
            {
                v_SessionID = sessionId,
                v_UserID = userId,
                v_MISAID = misaId,
                v_TenantID = tenantId,
                v_Expires = ticks,
                v_AccessToken = accessToken,
                v_MISAIDToken = misaIdToken,
                v_RenewSession = renewSession
            };
            var procName = isNewSession ? "Proc_Session_Insert" : "Proc_Session_Update";
            var success =
                await _databaseService.ExecuteUsingStoredProcedure(Guid.Empty, AppCode.Auth, procName, param);
            if (success)
            {
                await SetSessionForCache(sessionId, accessToken);
            }
        }

        private async Task SetSessionForCache(string sessionId, string accessToken)
        {
            var accessTokenCacheName = string.Format(CacheKey.CacheAccessToken, sessionId);
            await _cacheService.Set(accessTokenCacheName, accessToken, TimeSpan.FromHours(12),
                isAppendAppCodeToKey: false);
        }

        public async Task RemoveSession(string sessionId)
        {
            var param = new
            {
                v_SessionID = sessionId
            };
            await _databaseService.ExecuteUsingStoredProcedure(Guid.Empty, AppCode.Auth,
                "Proc_Session_DeleteBySessionID", param);
            var accessTokenCacheName = string.Format(CacheKey.CacheAccessToken, sessionId);
            await _cacheService.Delete(accessTokenCacheName, isAppendAppCodeToKey: false);
        }

        public async Task SetOptionLastApp(HttpResponse httpResponse, string lastAppCode, bool existedCookie)
        {
            throw new NotImplementedException();
        }

        public async Task SetConnectedApp(string sessionId, string appCode)
        {
            bool renewSession = false;
#if DEBUG
            renewSession = true;
#endif
            var param = new
            {
                v_SessionID = sessionId,
                v_AppCode = appCode,
                v_RenewSession = renewSession
            };
            await _databaseService.ExecuteUsingStoredProcedure(Guid.Empty, AppCode.Auth, "Proc_Session_Connected_Apps_Insert", param);
        }
    }
}