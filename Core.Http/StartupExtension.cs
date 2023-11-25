using AutoMapper.Configuration;
using Core.Http.Factorry;
using Core.Interface;
using Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Core.Http
{
    public static class StartupExtension
    {
        public static void UseHttpService(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetSection("AppSettings:HttpClientSetting:UseResilientHttp").Value == bool.TrueString)
            {
                services.AddSingleton<IResilientHttpClientFactory, ResilientHttpClientFactory>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogService>();
                    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                    var configService = sp.GetRequiredService<IConfigService>();
                    var retryCount = 6;
                    if (!string.IsNullOrEmpty(configuration["AppSettings:HttpClientSetting:HttpClientRetryCount"]))
                    {
                        retryCount = int.Parse(configuration["AppSettings:HttpClientSetting:HttpClientRetryCount"]);
                    }

                    var exceptionAllowedBeforeBreaking = 5;
                    if (!string.IsNullOrEmpty(
                        configuration["AppSettings:HttpClientSetting:HttpClientExceptionAllowedBeforeBreaking"]))
                    {
                        exceptionAllowedBeforeBreaking =
                            int.Parse(configuration[
                                "AppSettings:HttpClientSetting:HttpClientExceptionAllowedBeforeBreaking"]);
                    }

                    return new ResilientHttpClientFactory(logger, httpContextAccessor, configService,
                        exceptionAllowedBeforeBreaking, retryCount);
                });
                services.AddTransient<IHttpService, ResilientHttpClient>(sp =>
                    sp.GetService<IResilientHttpClientFactory>().CreateResilientHttpClient());
            }
            else
            {
                services.AddTransient<IHttpService, StandardHttpClient>();
            }
        }
    }
}