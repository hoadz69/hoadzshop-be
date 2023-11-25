using System;
using System.Threading.Tasks;
using Core.Model.Platform;
using Microsoft.AspNetCore.Http;

namespace Core.BL
{
    public interface ISessionBL
    {
        Task<string> GetAccessTokenBySessionId(string sessionId);

        Task<SC_AccessTokenByApp> GetAppInfoBySessionId(string sessionId, string appCode);

        Task<string> GetMisaIdTokenBySessionId(string sessionId);

        Task UpdateMisaIdTokenBySessionId(string sessionId, string misaIdToken);

        Task AddNewSession(string sessionId, bool isNewSession, Guid userId, Guid tenantId, long ticks,
            string accessToken, string appsToken, Guid misaId, string misaIdToken);

        Task RemoveSession(string sessionId);

        Task SetOptionLastApp(HttpResponse httpResponse, string lastAppCode, bool existedCookie);
        Task SetConnectedApp(string sessionId, string appCode);
    }
}