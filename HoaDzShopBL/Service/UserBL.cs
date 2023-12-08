using Core.BL;
using HoaDzShopBL.Interface;
using HoaDzShopCommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoaDzShopBL.Service
{
    public class UserBL : BaseHoadzShopBL, IUserBL
    {
        public UserBL(CoreServiceCollection serviceCollection) : base(serviceCollection)
        {

        }
        public bool login(UserLogin userLogin) 
        {
            var a = GetById<User>("db401a23-8d48-11ee-83f1-0242ac130003");
            return true;
        }
    }
}
