using System;
using System.Net.Http;
using Core.Interface;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Polly;

namespace Core.Http.Factorry
{
    public class ResilientHttpClientFactory : IResilientHttpClientFactory

    {
        private readonly int _retryCount;
        private readonly int _exceptionAllowedBeforeBreaking;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogService _logService;
        private readonly IConfigService _configService;

        public ResilientHttpClientFactory(ILogService logService, IHttpContextAccessor httpContextAccessor,  IConfigService configService,int retryCount = 6, int exceptionAllowedBeforeBreaking=5)
        {
            _retryCount = retryCount;
            _exceptionAllowedBeforeBreaking = exceptionAllowedBeforeBreaking;
            _httpContextAccessor = httpContextAccessor;
            _logService = logService;
            _configService = configService;
        }

        ResilientHttpClient IResilientHttpClientFactory.CreateResilientHttpClient()
        {
            return new ResilientHttpClient((origin) => CreatePolicies(),_httpContextAccessor, _configService, _logService);
        }
        private AsyncPolicy[] CreatePolicies() => new AsyncPolicy[]
        {
            Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    //number of retry
                    _retryCount,
                    //exponential backoff
                    retryAttem=>TimeSpan.FromSeconds(Math.Pow(2,retryAttem)),
                    //on retry
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"Retry {retryCount} implemented with Polly's RetryPolicy " +
                                  $"of {context.PolicyKey} " + $"at {context.OperationKey} " + $"due to: {exception}.";
 
                    }
                    ),
            Policy.Handle<HttpRequestException>().CircuitBreakerAsync(
                    //number of exceptions before breaking circuit
                    _exceptionAllowedBeforeBreaking,
                    // time circuit opend before retry
                    TimeSpan.FromMinutes(1),
                    (exception, duration) =>
                    {
                        //on circuit opened
                        //_logService.LogTrace("Circuit breaker opened");
                    },
                    () =>
                    {
                        //on circuit closed
                        //_logService.LogTrace("Circuit breaker reset");
                    }
                )
        };
    }
}