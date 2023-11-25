using System;
using System.ComponentModel.DataAnnotations;
using Core.Attribute;
using Core.Model;

namespace Core.Database
{
    [ConfigTable("shard_config")]
    [Serializable]
    public class ShardConfig : BaseModel
    {
        [Key]
        public int ShardId { get; set; }

        public string AppCode { get; set; }
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string TenantCode { get; set; }
        
    }
}