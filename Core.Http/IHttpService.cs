using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Core.Http
{
    public interface IHttpService
    {
        [Obsolete("Không sử dụng method này. Sử dụng CallInternalApi() hoặc CallExternalApi().")]
        Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers = null);

        [Obsolete("Không sử dụng method này. Sử dụng CallInternalApi() hoặc CallExternalApi().")]
        Task<HttpResponseMessage> PostAsync(string uri, object item, Dictionary<string, string> headers = null,
            string requestId = null);

        [Obsolete("Không sử dụng method này. Sử dụng CallInternalApi() hoặc CallExternalApi().")]
        Task<HttpResponseMessage> DeleteAsync(string uri, Dictionary<string, string> headers = null,
            string requestId = null);

        [Obsolete("Không sử dụng method này. Sử dụng CallInternalApi() hoặc CallExternalApi().")]
        Task<HttpResponseMessage> DeleteAsync(string uri, object item, Dictionary<string, string> headers = null,
            string requestId = null);

        [Obsolete("Không sử dụng method này. Sử dụng CallInternalApi() hoặc CallExternalApi().")]
        Task<HttpResponseMessage> PutAsync(string uri, object item, Dictionary<string, string> headers = null,
            string requestId = null);
    }
}