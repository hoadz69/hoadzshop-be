using Core.BL;
using HoaDzShopBL.Interface;
using HoaDzShopBL.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
namespace HoaDzShopBL
{
    public static class StartupExtension
    {
        public static void BaseStartupHoadzShop(this IServiceCollection services, IConfiguration configuration)
        {
            services.UseCoreBL(configuration);
            services.AddTransient<IUserBL, UserBL>();
        }
    }
}
