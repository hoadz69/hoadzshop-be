using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Core.Interface;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;

namespace Core.Http
{
    internal class ResilientHttpClient : BaseHttpClient, IHttpService
    {
        private readonly HttpClient _client;
        private readonly Func<string, IEnumerable<AsyncPolicy>> _policyCreator;
        private ConcurrentDictionary<string, AsyncPolicyWrap> _policyWrappers;
        public ResilientHttpClient(
            Func<string, IEnumerable<AsyncPolicy>> policyCreator,
            IHttpContextAccessor httpContextAccessor, 
            IConfigService configService, ILogService logService) 
            : base(httpContextAccessor, configService, logService)
        {
            HttpClientHandler httpClientHandler=new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => { return true; };
            httpClientHandler.SslProtocols =
                SslProtocols.Tls11 | SslProtocols.Tls | SslProtocols.Tls12 ;
            _client= new HttpClient(httpClientHandler);
            _policyCreator = policyCreator;
            _policyWrappers = new ConcurrentDictionary<string, AsyncPolicyWrap>();
        }


        public Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers = null)
        {
            var origin = GetOriginFromUri(uri);
            return HttpInvoker(origin, async (Context ctx) =>
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                SetRequestHeader(requestMessage, headers);
                LogRequest(requestMessage);
                var response = await _client.SendAsync(requestMessage);
                return response;
            });
        }

        public Task<HttpResponseMessage> PostAsync(string uri, object item, Dictionary<string, string> headers = null, string requestId = null)
        {
            return DoPostPutAsync(HttpMethod.Post, uri, item, headers, requestId);
        }

        public Task<HttpResponseMessage> DeleteAsync(string uri, Dictionary<string, string> headers = null, string requestId = null)
        {
            var origin = GetOriginFromUri(uri);
            return HttpInvoker(origin, async (Context ctx) =>
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                var requestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            
                SetRequestHeader(requestMessage, headers);
                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }

                LogRequest(requestMessage);
                return await _client.SendAsync(requestMessage);
                
            });
        }

        public Task<HttpResponseMessage> DeleteAsync(string uri, object item, Dictionary<string, string> headers = null, string requestId = null)
        {
            return DoPostPutAsync(HttpMethod.Delete, uri, item, headers, requestId);
        }

        public Task<HttpResponseMessage> PutAsync(string uri, object item, Dictionary<string, string> headers = null, string requestId = null)
        {
            return DoPostPutAsync(HttpMethod.Put, uri, item, headers, requestId);
        }

        private Task<HttpResponseMessage> DoPostPutAsync(HttpMethod method, string uri, object item,
            Dictionary<string, string> headers = null, string requestId = null)
        {
            var origin = GetOriginFromUri(uri);
            return HttpInvoker(origin, async (Context ctx) =>
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                var requestMessage = new HttpRequestMessage(method, uri);
            
            SetRequestHeader(requestMessage, headers);
            if (item is HttpContent)
            {
                requestMessage.Content = item as HttpContent;
            }
            else
            {
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(item), System.Text.Encoding.UTF8,
                    "application/json");
            }

            if (requestId != null)
            {
                requestMessage.Headers.Add("x-requestid", requestId);
            }

            LogRequest(requestMessage);
            var response = await _client.SendAsync(requestMessage);
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new HttpRequestException();
            }

            return response;
        });
        



    }

        private async Task<T> HttpInvoker<T>(string origin, Func<Context, Task<T>> action)
        {
            var normalizedOrigin = NormalizeOrigin(origin);
            if (!_policyWrappers.TryGetValue(normalizedOrigin, out AsyncPolicyWrap policyWrap))
            {
                policyWrap = Policy.WrapAsync(_policyCreator(normalizedOrigin).ToArray());
                _policyWrappers.TryAdd(normalizedOrigin, policyWrap);
            }

            return await policyWrap.ExecuteAsync(action, new Context(normalizedOrigin));
        }

        private string NormalizeOrigin(string origin)
        {
            return origin?.Trim()?.ToLower();
        }

        private string GetOriginFromUri(string uri)
        {
            var url=new Uri(uri);
            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";
            return origin;
        }
    }
}