using HoaDzShopCommon.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoaDzShopBL.Interface
{
    public interface IUserBL
    {
        bool login(UserLogin userLogin);
    }
}
