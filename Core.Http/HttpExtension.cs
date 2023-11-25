using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Core.Contant;
using Core.Enumeration;
using Core.Model;
using Core.Ultitily;

namespace Core.Http
{
    public static class HttpExtension
    {
        /// <summary>
        /// Method gọi api ở mức server - server
        /// </summary>
        /// <param name="httpService"></param>
        /// <param name="apiUrlKey">Key của ApiUrl (vd: ApiUrlKey.PlatformApi)</param>
        /// <param name="apiPath"> Path đến method của controller</param>
        /// <param name="method">Get/Post/Put/Delete</param>
        /// <param name="param">object cần truyền vào body</param>
        /// <param name="authorizationToken"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<ServiceResponse> CallInternalApi(this IHttpService httpService, string apiUrlKey,
            string apiPath, HttpMethod method, object param = null, string authorizationToken = null,
            Dictionary<string, string> headers = null)
        {
            try
            {
                var apiUrl = ((BaseHttpClient) httpService).GetApiUrl(apiUrlKey);
                var fullApiurl = (new StringBuilder((apiUrl.EndsWith("/")) ? apiUrl : (apiUrl + "/")))
                    .Append((apiPath.StartsWith("/") ? apiPath.Substring(1) : apiPath)).ToString();
                if (headers == null)
                {
                    headers=new Dictionary<string, string>();
                }

                headers.AddOrUpdate(Keys.InternalApiToken, ((BaseHttpClient) httpService).GetInternalApiToken());
                var sessionId = ((BaseHttpClient) httpService).GetSessionId();
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    headers.AddOrUpdate(Keys.SessionId,sessionId);
                }

                if (!string.IsNullOrWhiteSpace(authorizationToken))
                {
                    headers.AddOrUpdate(Keys.Authorization,authorizationToken);
                }
                else
                {
                    var accessToken = ((BaseHttpClient) httpService).GetAuthorizationToken();
                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        headers.AddOrUpdate(Keys.Authorization,accessToken);
                    }
                }

                var responseMessage = ExecuteRequest(httpService, fullApiurl, method, param: param, headers: headers);
                if (responseMessage.IsSuccessStatusCode)
                {
                    return Converter.Deserialize<ServiceResponse>(responseMessage.Content.ReadAsStringAsync().Result);
                }
                else
                {
                    return new ServiceResponse()
                    {
                        Success = false,
                        Code = Enumeration.ServiceResponseCode.Error,
                        SystemMessage = responseMessage.ReasonPhrase
                    };
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Method goji api ra mức server - ngoài
        /// </summary>
        /// <param name="httpService"></param>
        /// <param name="fullApiUrl">đầy đủ đường dẫn của api</param>
        /// <param name="method"></param>
        /// <param name="param"></param>
        /// <param name="authorizationToken"></param>
        /// <param name="headers"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<ServiceResponse> CallExternalApi<T>(this IHttpService httpService, string fullApiUrl, HttpMethod method, object param = null, string authorizationToken = null,
            Dictionary<string, string> headers = null)
        {
            try
            {
                var responseMessage =
                    ExecuteRequest(httpService, fullApiurl: fullApiUrl, method, param: param, headers: headers);
                var response= new ServiceResponse()
                {
                    Success = responseMessage.IsSuccessStatusCode,
                    Code = (responseMessage.IsSuccessStatusCode) ? Enumeration.ServiceResponseCode.Succes : ServiceResponseCode.Error,
                    SubCode = (responseMessage.IsSuccessStatusCode ? 0 : (int) responseMessage.StatusCode),
                    SystemMessage = responseMessage.ReasonPhrase,
                    UserMessage = (responseMessage.IsSuccessStatusCode ? "" : ServiceResponse.DEFAULT_ERRORMESSAGE)
                };
                if (response.Success)
                {
                    response.Data =
                        Converter.DeserializeObject(responseMessage.Content.ReadAsStringAsync().Result, typeof(T));
                }
                else
                {
                    try
                    {
                        response.Data = responseMessage.Content.ReadAsStringAsync().Result;
                    }
                    catch (Exception e)
                    {
                    }
                }

                return response;
            }
            catch (Exception e)
            {
                throw;
            }
            
        }
        private static HttpResponseMessage ExecuteRequest(IHttpService httpService, string fullApiurl, HttpMethod method, object param=null, Dictionary<string, string> headers=null)
        {
            
                HttpResponseMessage responseMessage = null;
                if (method == HttpMethod.Get)
                {
#pragma warning disable CS0618 //Type or memeber is obsolete
                    responseMessage = httpService.GetAsync(fullApiurl, headers: headers).Result;
#pragma warning restore CS0618 // type or member is obsolete
                
                }
                else
                {
                    if (method == HttpMethod.Post)
                    {
#pragma warning disable CS0618 //Type or memeber is obsolete
                        responseMessage = httpService.PostAsync(fullApiurl,param, headers: headers).Result;
#pragma warning restore CS0618 // type or member is obsolete
                    }
                    else if (method == HttpMethod.Delete)
                    {
#pragma warning disable CS0618 //Type or memeber is obsolete
                        responseMessage = httpService.DeleteAsync(fullApiurl, headers: headers).Result;
#pragma warning restore CS0618 // type or member is obsolete
                    }
                    else if (method == HttpMethod.Put)
                    {
#pragma warning disable CS0618 //Type or memeber is obsolete
                        responseMessage = httpService.PutAsync(fullApiurl,param, headers: headers).Result;
#pragma warning restore CS0618 // type or member is obsolete
                    }
                    else
                    {
                        throw new NotImplementedException($"HttpMethod '{ method.Method}' is not supported.");
                    }
                }

                return responseMessage;
           
        }
    }
}