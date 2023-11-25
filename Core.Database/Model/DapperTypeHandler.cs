using System.Collections.Generic;
using System.Data;

namespace Core.Database
{
    public class DapperTypeHandler : Dapper.SqlMapper.TypeHandler<System.Collections.Generic.IEqualityComparer<string>>
    {
        public override void SetValue(IDbDataParameter parameter, IEqualityComparer<string> value)
        {
            throw new System.NotImplementedException();
        }

        public override IEqualityComparer<string> Parse(object value)
        {
            throw new System.NotImplementedException();
        }
    }
}