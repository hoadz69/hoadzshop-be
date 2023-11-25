using System;
using System.Collections.Generic;

namespace Core.Model.Platform
{
    /// <summary>
    /// Danh sách quyền truy cập trong app
    /// </summary>
    public class SC_PermissionByApp
    {
        /// <summary>
        /// ID Cơ cấu tổ chức
        /// </summary>
        public Guid OrganizationUnitId { get; set; }
        /// <summary>
        /// Misa Code
        /// </summary>
        public string MisaCode { get; set; }
        
        /// <summary>
        /// Danh sách quyền theo màn hình
        /// </summary>
        public Dictionary<string,string> PermissionDetail { get; set; }

        public SC_PermissionByApp()
        {
            PermissionDetail=new Dictionary<string, string>();
        }
    }
}