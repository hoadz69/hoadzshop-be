using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using Core.Interface;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json; 

namespace Core.Http
{
    public class StandardHttpClient : BaseHttpClient, IHttpService
    {
        private HttpClient _client;
        public StandardHttpClient(IHttpContextAccessor httpContextAccessor, IConfigService configService, ILogService logService) : base(httpContextAccessor, configService, logService)
        {
            HttpClientHandler httpClientHandler=new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => { return true; };
            httpClientHandler.SslProtocols =
                SslProtocols.Tls11 | SslProtocols.Tls | SslProtocols.Tls12 ;
            _client= new HttpClient(httpClientHandler);
        }

        public async Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers = null)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                var requestMessage= new HttpRequestMessage(HttpMethod.Get, uri);
                SetRequestHeader(requestMessage,headers);
                LogRequest(requestMessage);
                return await _client.SendAsync(requestMessage);
        }

        public async Task<HttpResponseMessage> PostAsync(string uri, object item, Dictionary<string, string> headers = null, string requestId = null)
        {
            return await DoPostPutAsync(HttpMethod.Post, uri, item, headers, requestId);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string uri, Dictionary<string, string> headers = null, string requestId = null)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            var requestMessage= new HttpRequestMessage(HttpMethod.Delete, uri);
            SetRequestHeader(requestMessage,headers);
            if (requestId != null)
            {
                requestMessage.Headers.Add("x-requestid",requestId);
            }
            LogRequest(requestMessage);
            return await _client.SendAsync(requestMessage);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string uri, object item, Dictionary<string, string> headers = null, string requestId = null)
        {
            return await DoPostPutAsync(HttpMethod.Delete, uri, item, headers, requestId);
        }

        public async Task<HttpResponseMessage> PutAsync(string uri, object item, Dictionary<string, string> headers = null, string requestId = null)
        {
            return await DoPostPutAsync(HttpMethod.Put, uri, item, headers, requestId);
        }

        private async Task<HttpResponseMessage> DoPostPutAsync<T>(HttpMethod method, string uri, T item,
            Dictionary<string, string> headers = null, string requestId = null)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
            var requestMessage= new HttpRequestMessage(method, uri);
            SetRequestHeader(requestMessage,headers);
            if (item is HttpContent)
            {
                requestMessage.Content=item as HttpContent;
            }
            else
            {
                requestMessage.Content=new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8,"application/json");
            }
            LogRequest(requestMessage);
            
            var response= await _client.SendAsync(requestMessage);
            return response;
        }
    }
}