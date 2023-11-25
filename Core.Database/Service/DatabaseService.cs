using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Contant;
using Core.Database.SQLHelper;
using Core.Database.Utility;
using Core.Interface;
using Core.Model;
using Core.Services;
using Core.Ultitily;
using Core.Utility;
using Dapper;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;

namespace Core.Database
{
    public class DatabaseService : IDatabaseService
    {
        private const string PROC_GetShardConfigByTenantID = "Proc_GetShardConfigByTenantID";
        private readonly IHttpContextAccessor _httpContext;
        //private readonly IAuthService _authService;
        private readonly ICacheService _cacheService;
        private readonly IConfigService _configService;
        private string _modeNamespace;
        private bool _allowRunCommandWithoutParameter = false;
        private bool _allowRunCommandWithoutTenantIdCondition = false;

        private Dictionary<string, string> fixedConnectionString = new Dictionary<string, string>()
        {
            {AppCode.Auth, ConnectionStringsKey.AuthDb},
            {AppCode.Payment, ConnectionStringsKey.PaymentDb},
            {AppCode.Billing, ConnectionStringsKey.BillingDb},
            {AppCode.Workflow, ConnectionStringsKey.WorkflowDb},
        };
        //, IAuthService authService
        public DatabaseService(IHttpContextAccessor httpContext, ICacheService cacheService,
            IConfigService configService)
        {
            _httpContext = httpContext;
            //_authService = authService;
            _cacheService = cacheService;
            _configService = configService;
            _modeNamespace = _configService.GetAppSetting(AppSettingsKey.ModelNamespace);
            if (string.IsNullOrWhiteSpace(_modeNamespace))
            {
                _modeNamespace = DatabaseConstant.DefaultModelNamespace;
            }
        }

        public void RunCommandWithoutParameter(Action action)
        {
            _allowRunCommandWithoutParameter = true;
            action.Invoke();
            _allowRunCommandWithoutParameter = false;
        }

        public void RunCommandWithoutTenantIdCondition(Action action)
        {
            _allowRunCommandWithoutTenantIdCondition = true;
            action.Invoke();
            _allowRunCommandWithoutTenantIdCondition = false;
        }

        #region Get Method

        /// <summary>
        /// Lấy chi tiết model theo id
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> GetById<T>(Guid tenantId, string appCode, string id) where T : BaseModel
        {
            string commandText = string.Empty;
            var dicParam = new Dictionary<string, object>();
            GenerateSelectById<T>(ref commandText, ref dicParam, tenantId, id);
            var data = await QueryUsingCommandText<T>(tenantId, appCode, commandText, dicParam);
            return data?.FirstOrDefault();
        }

        /// <summary>
        /// Lấy chi tiết model theo id trả về dạng dynamic
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="typeName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<dynamic> GetById(Guid tenantId, string appCode, string typeName, string id)
        {
            Type modelType = GetModelType(typeName);
            return GetById(tenantId, appCode, modelType, id).Result;
        }

        /// <summary>
        /// Lấy chi tiết model theo id trả về dạng object
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="modelType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<object> GetById(Guid tenantId, string appCode, Type modelType, string id)
        {
            string commandText = string.Empty;
            var dicParam = new Dictionary<string, object>();
            GenerateSelectById(ref commandText, ref dicParam, tenantId, modelType, id);
            var data = await QueryUsingCommandText(tenantId, appCode, commandText, dicParam);
            return data.Count() > 0 ? data.First() : null;
        }

        /// <summary>
        /// Lấy danh sách model theo danh sách id
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="ids"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<List<T>> GetByListId<T>(Guid tenantId, string appCode, string ids) where T : BaseModel
        {
            string commandText = string.Empty;
            var dicParam = new Dictionary<string, object>();
            GenerateSelectAll<T>(ref commandText, ref dicParam, tenantId);
            ids = Converter.Base64Decode(ids);
            var listId = Converter.Deserialize<List<object>>(ids);
            for (int i = 0; i < listId.Count; i++)
            {
                if (i == 1)
                {
                    commandText += $" AND {typeof(T).GetPrimaryKeyFieldName()} IN (";
                }

                if (i > 1)
                {
                    commandText += ", ";
                }

                commandText += $@"IDValue{i.ToString()}";
                dicParam.Add($"IDValue{i.ToString()}", GetSqlValue(listId[i].ToString()));
                if (i == listId.Count)
                {
                    commandText += ")";
                }
            }

            var data = await QueryUsingCommandText<T>(tenantId, appCode, commandText, dicParam);
            return data;
        }


        /// <summary>
        /// Lấy tất cả dữ liệu của 1 bảng
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="filter">chuỗi base64 của filter where</param>
        /// <param name="sort">Chuỗi base64 của sort</param>
        /// <param name="customFilter">chuỗi base64 của customFilter</param>
        /// <param name="fixedFilter">thêm điều kiện filter</param>
        /// <param name="columns"></param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T">Tên view hoặc table ( không truyền sẽ lấy theo T)</typeparam>
        /// <returns></returns>
        public async Task<object> GetAll<T>(Guid tenantId, string appCode, string filter, string sort,
            string customFilter = "",
            WhereParameter fixedFilter = null, string columns = "", string viewOrTableName = "") where T : BaseModel
        {
            dynamic list = null;
            List<Type> types = null;
            string commandText = string.Empty;
            var dicParam = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(columns))
            {
                GenerateSelectAll<T>(ref commandText, ref dicParam, tenantId, viewOrTableName: viewOrTableName);
                types = new List<Type>() {typeof(T), typeof(TotalData)};
            }
            else
            {
                GenerateSelectByColumn<T>(ref commandText, ref dicParam, tenantId, columns,
                    viewOrTableName: viewOrTableName);
                types = new List<Type>() {typeof(object), typeof(TotalData)};
            }

            var filterWhere = GridFilterParser.Parse(DecodeBase64Param(filter), DecodeBase64Param(customFilter));
            if (filterWhere != null)
            {
                commandText += $" AND ({filterWhere.WhereClause})";
                dicParam.AddOrUpdate(filterWhere.WhereValues);
            }

            if (fixedFilter != null)
            {
                commandText += $" AND ({fixedFilter.WhereClause})";
                dicParam.AddOrUpdate(fixedFilter.WhereValues);
            }

            sort = DecodeBase64Param(sort);
            GeneratePaging(ref commandText, 1, DatabaseConstant.MaxReturnRecord, sort);
            list = await QueryUsingCommandText<T>(tenantId, appCode, commandText, dicParam);
            return list;
        }


        /// <summary>
        /// Lấy tất cả dữ liệu từ một bảng
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="typeName"></param>
        /// <param name="filter">chuỗi base64 của filter where</param>
        /// <param name="sort">chuỗi base64 của sort</param>
        /// <param name="customFilter">base 64 của customfilter</param>
        /// <param name="fixedFilter"> thêm điều kiện</param>
        /// <param name="columns">các column cần lấy, ngăn cách dấu ","</param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<object> GetAll(Guid tenantId, string appCode, string typeName, string filter, string sort,
            string customFilter = "",
            WhereParameter fixedFilter = null, string columns = "", string viewOrTableName = "")
        {
            dynamic list = null;
            List<Type> types = null;
            string commandText = string.Empty;
            var dicParam = new Dictionary<string, object>();
            var modelType = GetModelType(typeName);
            if (string.IsNullOrEmpty(columns))
            {
                GenerateSelectAll(ref commandText, ref dicParam, modelType, tenantId: tenantId,
                    viewOrTableName: viewOrTableName);
                types = new List<Type>() {modelType, typeof(TotalData)};
            }
            else
            {
                GenerateSelectByColumn(ref commandText, ref dicParam, typeModel: modelType, tenantId, columns,
                    viewOrTableName: viewOrTableName);
                types = new List<Type>() {typeof(object), typeof(TotalData)};
            }

            var filterWhere = GridFilterParser.Parse(DecodeBase64Param(filter), DecodeBase64Param(customFilter));
            if (filterWhere != null)
            {
                commandText += $" AND ({filterWhere.WhereClause})";
                dicParam.AddOrUpdate(filterWhere.WhereValues);
            }

            if (fixedFilter != null)
            {
                commandText += $" AND ({fixedFilter.WhereClause})";
                dicParam.AddOrUpdate(fixedFilter.WhereValues);
            }

            sort = DecodeBase64Param(sort);
            GeneratePaging(ref commandText, 1, DatabaseConstant.MaxReturnRecord, sort);
            list = await QueryUsingCommandText(tenantId, appCode, commandText, dicParam);
            return list;
        }


        public async Task<PagingResponse> GetPagingUsingCommandText(Type typeModel, Guid tenantId, string appCode,
            int pageSize,
            int pageIndex,
            WhereParameter filter, List<GridSortItem> sorts, WhereParameter customFilter,
            WhereParameter fixedFilter = null, string columns = "",
            string viewOrTableName = "")
        {
            PagingResponse res = null;
            List<List<object>> queryResult = null;
            string commandText = "";
            var dicParam = new Dictionary<string, object>();
            List<Type> types = null;
            if (string.IsNullOrEmpty(columns))
            {
                GenerateSelectAll(ref commandText, ref dicParam, typeModel, tenantId, viewOrTableName);
                types = new List<Type>() {typeModel, typeof(TotalData)};
            }
            else
            {
                GenerateSelectByColumn(ref commandText, ref dicParam, typeModel, tenantId, columns, viewOrTableName);
            }

            string commandTextTotal = "";
            GenerateSelectCountAll(ref commandTextTotal, ref dicParam, typeModel, tenantId, viewOrTableName);
            if (filter != null)
            {
                filter.AddWhere(customFilter);
            }
            else
            {
                filter = customFilter;
            }

            if (filter != null)
            {
                commandText += $" AND ({filter.WhereClause})";
                commandTextTotal += $" AND ({filter.WhereClause})";
                dicParam.AddOrUpdate(filter.WhereValues);
            }

            if (fixedFilter != null)
            {
                commandText += $" AND ({fixedFilter.WhereClause})";
                commandTextTotal += $" AND ({fixedFilter.WhereClause})";
                dicParam.AddOrUpdate(fixedFilter.WhereValues);
            }

            GeneratePaging(ref commandText, pageIndex, pageSize, sorts);
            commandText += $"{commandText}; {commandTextTotal};";
            queryResult = await QueryMultipleUsingCommandText(tenantId, appCode, types, commandText, dicParam);
            if (queryResult?.Count > 0)
            {
                res = new PagingResponse(queryResult[0].ToList(),
                    queryResult[1].Cast<TotalData>().FirstOrDefault().Total, IsExportData());
            }

            return res;
        }

        // public async Task<PagingResponse> GetPagingUsingCommandText(Type typeModel, Guid tenantId, string appCode, int pageSize, int pageIndex,
        //     string filter, string sort, string customFilter, WhereParameter fixedFilter = null, string columns = "",
        //     string viewOrTableName = "")
        // {
        //     var filterWhere = GridFilterParser.Parse(DecodeBase64Param(filter), DecodeBase64Param(customFilter));
        //     List<GridSortItem> sorts = Converter.Deserialize<List<GridSortItem>>(DecodeBase64Param(sort));
        //     return await GetPagingUsingCommandText(typeModel, tenantId, appCode, pageSize, pageIndex, filterWhere,
        //         sorts, null, fixedFilter, column, viewOrTableName);
        // }


        /// <summary>
        /// Generate câu lệnh count từ entity
        /// </summary>
        /// <param name="commandTextTotal"></param>
        /// <param name="dicParam"></param>
        /// <param name="typeModel"></param>
        /// <param name="tenantId"></param>
        /// <param name="viewOrTableName"></param>
        private void GenerateSelectCountAll(ref string commandTextTotal, ref Dictionary<string, object> dicParam,
            Type typeModel, Guid tenantId, string viewOrTableName = "")
        {
            if (string.IsNullOrEmpty(viewOrTableName))
            {
                viewOrTableName = typeModel.GetViewOrTableName();
            }

            commandTextTotal =
                $" SELECT COUNT(1) AS Total FROM `{SecureUtil.SafeSqlLiternalForObjectName(viewOrTableName)}`";
            ProcessConditionTenantId(ref commandTextTotal, ref dicParam, tenantId);
        }


        /// <summary>
        /// Get paging using commandText
        /// </summary>
        /// <param name="typeModel"></param>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="pageSize">Số bản ghi/trang</param>
        /// <param name="pageIndex"></param>
        /// <param name="filter">chuỗi base 64 của filter where</param>
        /// <param name="sort">chuỗi base64 của sort</param>
        /// <param name="customFilter">chuỗi base64 của custom filter</param>
        /// <param name="fixedFilter">điều kiện where fix ẵn</param>
        /// <param name="columns">Các column cần lấy</param>
        /// <param name="viewOrTableName">Tên view or table name ( không truyền sẽ mặc định lấy theo typeModel) </param>
        /// <returns></returns>
        public async Task<PagingResponse> GetPagingUsingCommandText(Type typeModel, Guid tenantId, string appCode,
            int pageSize,
            int pageIndex,
            string filter, string sort, string customFilter,
            WhereParameter fixedFilter = null, string columns = "",
            string viewOrTableName = "")
        {
            var filterWhere = GridFilterParser.Parse(DecodeBase64Param(filter), DecodeBase64Param(customFilter));
            List<GridSortItem> sortItems = Converter.Deserialize<List<GridSortItem>>(DecodeBase64Param(sort));
            return await GetPagingUsingCommandText(typeModel, tenantId, appCode, pageSize, pageIndex, filterWhere,
                sortItems, null, fixedFilter, columns, viewOrTableName);

        }

        /// <summary>
        /// Generate paging sử dụng command text
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="pageSize">Số bản ghi/trang</param>
        /// <param name="pageIndex">số thứ tự trang</param>
        /// <param name="filter">chuỗi base64 filter</param>
        /// <param name="sorts"></param>
        /// <param name="customFilter">chuỗi base64 của customFilter</param>
        /// <param name="column"></param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<PagingResponse> GetPagingUsingCommandText<T>(Guid tenantId, string appCode, int pageSize,
            int pageIndex, WhereParameter filter,
            List<GridSortItem> sorts, WhereParameter customFilter, WhereParameter fixedFilter = null,
            string columns = "", string viewOrTableName = "")
            where T : BaseModel
        {
            return await GetPagingUsingCommandText(typeof(T), tenantId, appCode, pageSize, pageIndex, filter, sorts,
                customFilter, fixedFilter, columns, viewOrTableName);
        }

        /// <summary>
        /// Generate paging sử dụng command text
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="pageSize">Số bản ghi/trang</param>
        /// <param name="pageIndex">số thứ tự trang</param>
        /// <param name="filter">chuỗi base64 filter</param>
        /// <param name="sort">chuỗi base64 của sort</param>
        /// <param name="customFilter">chuỗi base64 của customFilte</param>
        /// <param name="fixedFilter">điều kiện where fix sẵn</param>
        /// <param name="columns">Các column ngăn cách dấu ,</param>
        /// <param name="viewOrTableName">tên view hoặ table ( không truyền thì sẽ lấy theo T)</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<PagingResponse> GetPagingUsingCommandText<T>(Guid tenantId, string appCode, int pageSize,
            int pageIndex, string filter,
            string sort, string customFilter, WhereParameter fixedFilter = null, string columns = "",
            string viewOrTableName = "") where T : BaseModel
        {
            return GetPagingUsingCommandText(typeof(T), tenantId, appCode, pageSize, pageIndex, filter, sort,
                customFilter, fixedFilter, columns, viewOrTableName).Result;

        }

        /// <summary>
        /// Get paging sử dụng store procedure
        /// </summary>
        /// <param name="typeModel">Loại model</param>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="customFilter"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="fixedFilter"></param>
        /// <param name="column"></param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<PagingResponse> GetPagingUsingStoredProcedure<T>(Type typeModel, Guid tenantId,
            string appCode,
            int pageSize, int pageIndex,
            string filter, string sort, string customFilter, string storedProcedureName = "",
            Dictionary<string, object> param = null,
            WhereParameter fixedFilter = null, string column = "", string viewOrTableName = "") where T : BaseModel
        {
            return GetPagingUsingStoredProcedure(typeof(T), tenantId, appCode, pageSize, pageIndex, filter, sort,
                customFilter, storedProcedureName, param).Result;
        }

        public async Task<PagingResponse> GetPagingUsingStoredProcedure<T>(Guid tenantId, string appCode, int pageSize, int pageIndex, string filter,
            string sort, string customFilter, string storedProcedureName = "", Dictionary<string, object> param = null) where T : BaseModel
        {
            return GetPagingUsingStoredProcedure(typeof(T), tenantId, appCode, pageSize, pageIndex, filter, sort,
                customFilter, storedProcedureName, param).Result;
        }

        /// <summary>
        /// Get Paging sử dụng StoredProcedure
        /// </summary>
        /// <param name="typeModel"></param>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="customFilter"></param>
        /// <param name="storedProcedureName">Tên store phân tang ( không truyền sẽ tự động gọi "Proc_{loại model}_GetPaging</param>
        /// <param name="param"></param>
        /// <param name="fixedFilter"></param>
        /// <returns></returns>
        public async Task<PagingResponse> GetPagingUsingStoredProcedure(Type typeModel, Guid tenantId, string appCode,
            int pageSize, int pageIndex,
            string filter, string sort, string customFilter, string storedProcedureName = "",
            Dictionary<string, object> param = null,
            WhereParameter fixedFilter = null)
        {
            PagingResponse res = null;
            List<List<object>> queryResult = null;
            if (string.IsNullOrWhiteSpace(storedProcedureName))
            {
                storedProcedureName = GenerateGetPagingSp(typeModel);
            }

            string whereClause = "";
            var dicParam = new Dictionary<string, object>();
            List<Type> types = null;
            ProcessConditionTenantId(ref whereClause, ref dicParam, tenantId);
            var fullWhere = new WhereParameter(whereClause, dicParam);
            var filterWhere = GridFilterParser.Parse(DecodeBase64Param(filter), DecodeBase64Param(customFilter));
            if (fixedFilter != null)
            {
                fullWhere.AddWhere(fixedFilter);
            }

            whereClause = WhereParameter.Compile(fullWhere);
            string pagingClause = "";
            sort = DecodeBase64Param(sort);
            GeneratePaging(ref pagingClause, pageIndex, pageSize, sort);
            types = new List<Type>() {typeof(object), typeof(TotalData)};
            queryResult = await QueryMultipleUsingStoredProcedure(tenantId, appCode, types, storedProcedureName,
                param: Converter.ConvertDatabaseParam(
                    new {v_TenantId = tenantId, v_Where = whereClause, v_Paging = pagingClause}, param));
            if (queryResult?.Count > 0)
            {
                res = new PagingResponse(queryResult[0].ToList(),
                    queryResult[1].Cast<TotalData>().FirstOrDefault().Total, IsExportData());
            }

            return res;


        }

        /// <summary>
        /// generate command text lấy paging
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private string GenerateGetPagingSp<T>() where T : BaseModel
        {
            var sql = GenerateGetPagingSp(typeof(T));
            return sql;
        }
        /// <summary>
        /// Generate store lấy paging
        /// </summary>
        /// <param name="typeModel"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string GenerateGetPagingSp(Type typeModel)
        {
            var sql = string.Format(DatabaseConstant.SQLPagingSpTemplate, typeModel.Name);
            return sql;
        }

        /// <summary>
        /// lấy danh sách detail theo master id
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="modelType">loại model</param>
        /// <param name="masterKey">Giá trị khoá chính của model</param>
        /// <param name="filter">chuỗi base64 của filter where</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<dynamic> GetDetailByMaster(Guid tenantId, string appCode, string modelType, string masterKey,
            string filter)
        {
            string commandText = "";
            var dicParam = new Dictionary<string, object>();
            GenerateSelectDetailByTypeAndForeignKey(ref commandText, ref dicParam, tenantId, modelType, masterKey);
            filter = DecodeBase64Param(filter);
            WhereParameter whereFilter = GridFilterParser.Parse(filter);
            if (whereFilter != null)
            {
                commandText += $" AND ({whereFilter.WhereClause})";
                dicParam.AddOrUpdate(whereFilter.WhereValues);
            }

            var data = await QueryUsingCommandText(tenantId, appCode, commandText, dicParam);
            return data;
        }

        /// <summary>
        /// Sinh câu lệnh select theo khoá ngoại
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="tenantId"></param>
        /// <param name="typeName"></param>
        /// <param name="masterKey"></param>
        private void GenerateSelectDetailByTypeAndForeignKey(ref string commandText,
            ref Dictionary<string, object> dicParam, Guid tenantId, string typeName, string masterKey)
        {
            Type type = GetModelType(typeName);
            string viewOrTableName = type.GetViewOrTableName();
            commandText = $"SELECT t.* FROM `{SecureUtil.SafeSqlLiternalForObjectName(viewOrTableName)}` AS t ";
            ProcessConditionTenantId(ref commandText, ref dicParam, tenantId);
            commandText += $" AND {type.GetForeignKeyFieldName()} = @ForeignKeyID";
            commandText += $" ORDER BY ModifiedDate DESC";
            dicParam.Add("@ForeignKeyID", GetSqlValue(masterKey));
        }

        /// <summary>
        /// Lấy danh sách các loại detail theo id master
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="modelType"></param>
        /// <param name="masterKey">giá trị khoá chính của master mdoel</param>
        /// <param name="detailTypes">các loại detail model</param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<object>>> GetListDetailByMaster(Guid tenantId, string appCode, string masterKey, List<string> detailTypes)
        {
            List<string> keys = new List<string>();
            string commandText = "";
            var dicParam = new Dictionary<string, object>();
            foreach (var detail in detailTypes)
            {
                keys.Add(detail);
                GenerateSelectDetailByTypeAndForeignKey(ref commandText, ref dicParam, tenantId, detail, masterKey);
                commandText += ";";
            }

            var data = await QueryMultipleUsingCommandText(tenantId, appCode, keys, commandText, dicParam);
            return data;
        }

        /// <summary>
        /// Lấy danh sách detail theo master
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="masterType">loại của master</param>
        /// <param name="masterKey">giá trị key master</param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<object>>> GetListDetailByMaster(Guid tenantId, string appCode,
            Type masterType,
            string masterKey)
        {
            string commandText = "";
            var dicParam = new Dictionary<string, object>();
            var baseModel = (BaseModel) Activator.CreateInstance(masterType);
            var detailTypes = new List<string>();
            foreach (var detailConfig in baseModel.ModelDetailConfigs)
            {
                if (!string.IsNullOrWhiteSpace(detailConfig.PropertyOnMasterModel))
                {
                    var listTypes = baseModel.GetType().GetPropertyType(detailConfig.PropertyOnMasterModel);
                    detailTypes.Add(listTypes.GetGenericArguments().Single().Name);
                }
            }

            GenerateSelectDetailByTypeMaster(ref commandText, ref dicParam, tenantId, masterType, masterKey);
            var data = await QueryMultipleUsingCommandText(tenantId, appCode, detailTypes, commandText, dicParam);
            return data;
        }

        #endregion

        




        #region GetConnectionString

        /// <summary>
        /// Lấy đối tượng Sql connection
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private IDbConnection GetConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }
        /// <summary>
        /// Lấy đối tượng SQL connection
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <returns></returns>
        public IDbConnection GetConnection(Guid tenantId, string appCode)
        {
            string connectionString = GetConnectionString(tenantId, appCode);
            return new MySqlConnection(connectionString);
        }

        /// <summary>
        /// Lấy connection string connect tới database
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <returns></returns>
        private string GetConnectionString(Guid tenantId, string appCode)
        {
            List<ShardConfig> shardConfigs = null;
            string connection = "";
            if (!string.IsNullOrWhiteSpace(appCode)&& fixedConnectionString.ContainsKey(appCode))
            {
                return _configService.GetConnectionString(fixedConnectionString[appCode]);
            }
            //Get setting from HttpContext
            string masterDbConnectionString = _configService.GetConnectionString(ConnectionStringsKey.MasterDb);
            if (tenantId == Guid.Empty || string.IsNullOrWhiteSpace(appCode))
            {
                return masterDbConnectionString;
            }

            string cacheKey = string.Format(CacheKey.CacheShardConnection, tenantId);
            shardConfigs = _cacheService.Get<List<ShardConfig>>(cacheKey, false).Result;
            //Get cache data from Redis
#if DEBUG
            shardConfigs = null;
#endif
            if (shardConfigs != null)
            {
                // Lấy connection từ danh sách dựa vào loại database và luồng đọc/ghi
                connection = GetConnectionString(appCode, shardConfigs);
                if (connection == null)
                {
                    shardConfigs = GetTenantShardConfigForCache(tenantId, masterDbConnectionString, cacheKey);
                }
                else
                {
                    return connection;
                }
            }
            else //Nếu không có trong cache thì lấy từ database và set lại vào cache
            {
                shardConfigs = GetTenantShardConfigForCache(tenantId, masterDbConnectionString, cacheKey);
                return connection;
            }

            return connection;
        }

        private List<ShardConfig> GetTenantShardConfigForCache(Guid tenantId, string masterDbConnectionString, string cacheKey)
        {
            List<ShardConfig> shardConfigs = GetTenantShardConfig(tenantId, masterDbConnectionString);
            if (shardConfigs != null && shardConfigs.Count > 0)
            {
                var cacheData = shardConfigs.Select(c => new
                {
                    AppCode = c.AppCode,
                    Server = c.Server,
                    Database = c.Database,
                    UserId = c.UserId,
                    Password = c.Password
                }).ToList();
                _cacheService.Set(cacheKey, cacheData, isAppendAppCodeToKey: false);
            }

            return shardConfigs;
        }

        /// <summary>
        /// Lấy danh sách connection string của một tenant
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="masterDbConnectionString"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private List<ShardConfig> GetTenantShardConfig(Guid tenantId, string masterConnectionString)
        {
            var configs=new List<ShardConfig>();
            using (var cnn=new MySqlConnection(masterConnectionString))
            {
                var param = new {v_TenantId = tenantId};
                var query = cnn.Query<ShardConfig>(PROC_GetShardConfigByTenantID, param: param,
                    commandType: CommandType.StoredProcedure);
                configs = query.ToList();
            }

            return configs;
        }

        /// <summary>
        /// Lấy connection string theo database từ danh sách config
        /// </summary>
        /// <param name="appCode"></param>
        /// <param name="configs"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string GetConnectionString(string appCode, List<ShardConfig> configs)
        {
#if DEBUG
            var currentAppCode = _configService.GetAppSetting(AppSettingsKey.ApplicationCode);
            if (!Common.CheckAccessableDatabase(currentAppCode, appCode))
            {
                throw new Exception("DEV: Không được phép chạy quẻy trực tiếp trên 1 csdl của ứng dụng khác. Yêu cầu sử dụng CallInternalAPI().");
            }
#endif
            var config = configs.FirstOrDefault(c => c.AppCode.Equals(appCode, StringComparison.OrdinalIgnoreCase));
            if (config != null && !string.IsNullOrWhiteSpace(config.Server) &&
                !string.IsNullOrWhiteSpace(config.Database) && !string.IsNullOrWhiteSpace(config.UserId) &&
                !string.IsNullOrWhiteSpace(config.Password))
            {
                var builder= new MySqlConnectionStringBuilder()
                {
                    Server = config.Server,
                    Database = config.Database,
                    UserID = config.UserId,
                    Password = config.Password,
                    AllowUserVariables = true
                };
                return builder.ToString();
            }

            return null;
        }

        #endregion
        
        #region Execute Method
        /// <summary>
        /// Thực thi một commandText trả về thành công hay thất bại
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteUsingCommandText(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            bool success = true;
            var cd = BuildCommandDefinition(commandText, CommandType.Text, dicParam, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                success = await con.ExecuteAsync(cd) > 0;
            }
            else
            {
                using (var cnn = GetConnection(tenantId, appCode))
                {
                    success = await cnn.ExecuteAsync(cd) > 0;
                }
            }

            return success;
        }



        /// <summary>
        /// Thực thi một storedProcedure trả về thành công, thất bại
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteUsingStoredProcedure(Guid tenantId, string appCode, string storedProcedureName,
            object dicParam = null,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            bool success = true;
            var cd = BuildCommandDefinition(storedProcedureName, CommandType.StoredProcedure, dicParam, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                success = await con.ExecuteAsync(cd) > 0;
            }
            else
            {
                using (var cnn = GetConnection(tenantId, appCode))
                {
                    success = await cnn.ExecuteAsync(cd) > 0;
                }
            }

            return success;
        }

        /// <summary>
        /// Thực thi 1 command trả về 1 giá trị
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ExecuteScalarUsingCommandText<T>(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam = null, IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            T result = default(T);
            var cd = BuildCommandDefinition(commandText, CommandType.Text, dicParam, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                result = await con.ExecuteScalarAsync<T>(cd);
            }
            else
            {
                using (var cnn = GetConnection(tenantId, appCode))
                {
                    result = await cnn.ExecuteScalarAsync<T>(cd);
                }
            }

            return result;
        }

        /// <summary>
        /// Thực thi 1 stored trả về 1 giá trị
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ExecuteScalarUsingStoredProcedure<T>(Guid tenantId, string appCode, string storedProcedureName,
            object param = null, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            T result = default(T);
            var cd = BuildCommandDefinition(storedProcedureName, CommandType.StoredProcedure, param, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                result = await con.ExecuteScalarAsync<T>(cd);
            }
            else
            {
                using (var cnn = GetConnection(tenantId, appCode))
                {
                    result = await cnn.ExecuteScalarAsync<T>(cd);
                }
            }

            return result;
        }

        /// <summary>
        /// Xoá 1 model
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="baseModel"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteDelete(Guid tenantId, string appCode, BaseModel baseModel,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            string commandText = string.Empty;
            var dicParam=new Dictionary<string,object>();
            GenerateDelete(ref commandText, ref dicParam, tenantId, baseModel);
            bool success = await ExecuteUsingCommandText(tenantId, appCode, commandText, dicParam,
                transaction: transaction, connection: connection);
            return success;
        }

        

        /// <summary>
        /// Xoá 1 model
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<bool> ExecuteDelete<T>(Guid tenantId, string appCode, T model,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return await ExecuteDelete(tenantId, appCode, model, transaction, connection);
        }
        
        

        /// <summary>
        /// Xoá bản ghi theo master id
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="modelType">loại model</param>
        /// <param name="fieldName">tên field làm điều kiện xoá</param>
        /// <param name="fieldValue">giá trị của field điều kiện</param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteDeleteByFieldNameAndValue(Guid tenantId, string appCode, Type modelType,
            string fieldName,
            object fieldValue, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            return await ExecuteDeleteByFieldNameAndValue(tenantId, appCode, modelType.GetTableNameOnly(), fieldName,
                fieldValue, transaction, connection);
        }

        /// <summary>
        /// xoá bản ghi theo masterID
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="tableName">tên bảng trong database</param>
        /// <param name="fieldName">tên field làm điều kiện xoá</param>
        /// <param name="fieldValue">giá trị của field điều kiện</param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteDeleteByFieldNameAndValue(Guid tenantId, string appCode, string tableName, string fieldName,
            object fieldValue, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            string commandText = string.Empty;
            var dicParam=new Dictionary<string,object>();
            GenerateDeleteByFieldNameAndValue(ref commandText, ref dicParam, tenantId, tableName, fieldName,
                fieldValue);
            bool success = await ExecuteUsingCommandText(tenantId, appCode, commandText, dicParam,
                transaction: transaction, connection: connection);
            return success;
        }

        

        /// <summary>
        /// Update giá trị từng trường theo trường khoá
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="modelType"></param>
        /// <param name="fieldName"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> ExecuteUpdateField(Guid tenantId, string appCode, FieldUpdate fieldUpdate,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            string commandText = string.Empty;
            var dicParam=new Dictionary<string,object>();
            GenerateUpdateByFieldNameAndValue(ref commandText, ref dicParam,fieldUpdate, tenantId);
            return await ExecuteUsingCommandText(tenantId, appCode, commandText, dicParam, transaction, connection);
        }
        
        public async Task<bool> ExecuteCheckDuplicateDate(Guid tenantId, string appCode, string tableName, string fieldKey,
            dynamic valueKey,
            Dictionary<string, object> fieldUnique, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            string commandText = string.Empty;
            var dicParam=new Dictionary<string,object>();
            GenerateCheckDuplicate(ref commandText, ref dicParam, tenantId, tableName, fieldKey, valueKey, fieldUnique);
            return await ExecuteScalarUsingCommandText<bool>(tenantId, appCode, commandText, dicParam, transaction, connection);
        }

        /// <summary>
        /// Thực hiện câu lệnh check trùng bản ghi
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="tenantId"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldKey"></param>
        /// <param name="valueKey"></param>
        /// <param name="fieldUnique"></param>
        private void GenerateCheckDuplicate(ref string commandText, ref Dictionary<string, object> dicParam, Guid tenantId, string tableName, string fieldKey, object valueKey, Dictionary<string, object> fieldUnique)
        {
            var fieldUpdates= new List<string>();
            foreach (var item in fieldUnique)
            {
                fieldUpdates.Add($"LOWER(`{SecureUtil.SafeSqlLiternalForObjectName(item.Key)}`) = @{item.Key}");
                var value = item.Value;
                if (value is string)
                {
                    value = value.ToString().ToLower();
                }
                dicParam.Add($@"{item.Key}",value);
            }

            commandText = $"SELECT 1 FROM `{SecureUtil.SafeSqlLiternalForObjectName(tableName)}` ";
            ProcessConditionTenantId(ref commandText, ref dicParam, tenantId);
            commandText += $" AND `{SecureUtil.SafeSqlLiternalForObjectName(fieldKey)}` <> @KeyValue";
            commandText += $" AND {string.Join(" AND ",fieldUpdates)}";
            commandText += $" COLLATE utf8mb4_bin";
            dicParam.Add("@KeyValue", valueKey);
        }
        #endregion

        #region Query Method

        

        /// <summary>
        /// Thực thi một commandText trả về danh sáhc
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<List<T>> QueryUsingCommandText<T>(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            List<T> result=new List<T>();
            var cd = BuildCommandDefinition(commandText, CommandType.Text, dicParam, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                var query = await con.QueryAsync<T>(cd);
                result = query.ToList();
            }
            else
            {
                using (var cnn= GetConnection(tenantId,appCode))
                {
                    var query = await cnn.QueryAsync<T>(cd);
                    result = query.ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Thực thi 1 storedProcedure trả về danh sách
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<List<T>> QueryUsingStoredProcedure<T>(Guid tenantId, string appCode, string storedProcedureName,
            object param = null,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            List<T> result=new List<T>();
            var cd = BuildCommandDefinition(storedProcedureName, CommandType.StoredProcedure, param, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                var query = await con.QueryAsync<T>(cd);
                result = query.ToList();
            }
            else
            {
                using (var cnn= GetConnection(tenantId,appCode))
                {
                    var query = await cnn.QueryAsync<T>(cd);
                    result = query.ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Thực thi một commandText trả về danh sách dynamic
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> QueryUsingCommandText(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            IEnumerable<dynamic> result=null;
            var cd = BuildCommandDefinition(commandText, CommandType.Text, dicParam, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                result = await con.QueryAsync(cd);
            }
            else
            {
                using (var cnn= GetConnection(tenantId,appCode))
                {
                    result = await cnn.QueryAsync(cd);
                }
            }

            return result;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về danh sách dạng dynamic
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<IEnumerable<dynamic>> QueryUsingStoredProcedure(Guid tenantId, string appCode,
            string storedProcedureName, object param = null,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            IEnumerable<dynamic> result=null;
            var cd = BuildCommandDefinition(storedProcedureName, CommandType.StoredProcedure, param, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                result = await con.QueryAsync(cd);
            }
            else
            {
                using (var cnn= GetConnection(tenantId,appCode))
                {
                    result = await cnn.QueryAsync(cd);
                }
            }

            return result;
        }

        /// <summary>
        /// Thực thi một commandText trả về danh sách dynamic
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="modelType"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<IEnumerable<object>> QueryUsingCommandText(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam, Type modelType, IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            IEnumerable<dynamic> result=null;
            var cd = BuildCommandDefinition(commandText, CommandType.Text, dicParam, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                result = await con.QueryAsync(modelType,cd);
            }
            else
            {
                using (var cnn= GetConnection(tenantId,appCode))
                {
                    result = await cnn.QueryAsync(modelType,cd);
                }
            }

            return result;
        }
        /// <summary>
        /// Thực thi một commandText trả về nhiều danh sách dữ liệu khác nhau
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="types">Các loại dữ liệu trả về</param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<List<List<object>>> QueryMultipleUsingCommandText(Guid tenantId, string appCode, List<Type> types,
            string commandText, Dictionary<string, object> dicParam,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            List<List<object>>res = new List<List<object>>();
            var cd = BuildCommandDefinition(commandText, CommandType.Text, dicParam, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                using (var multi= con.QueryMultiple(cd))
                {
                    int index = 0;
                    do
                    {
                        res.Add(multi.Read(types[index]).ToList());
                        index++;
                    } while (!multi.IsConsumed);
                }
            }
            else
            {
                using (var cnn= GetConnection(tenantId,appCode))
                {
                    using (var multi= cnn.QueryMultiple(cd))
                    {
                        int index = 0;
                        do
                        {
                            
                            res.Add(multi.Read(types[index]).ToList());
                            index++;
                        } while (!multi.IsConsumed);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về nhiều kiểu dữ liệu khác nhau
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="types">các loại dữ liệu trả về</param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<List<List<object>>> QueryMultipleUsingStoredProcedure(Guid tenantId, string appCode,
            List<Type> types, string storedProcedureName,
            object param = null, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            List<List<object>>res = new List<List<object>>();
            var cd = BuildCommandDefinition(storedProcedureName, CommandType.StoredProcedure, param, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                using (var multi= con.QueryMultiple(cd))
                {
                    int index = 0;
                    do
                    {
                        res.Add(multi.Read(types[index]).ToList());
                        index++;
                    } while (!multi.IsConsumed);
                }
            }
            else
            {
                using (var cnn= GetConnection(tenantId,appCode))
                {
                    using (var multi= cnn.QueryMultiple(cd))
                    {
                        int index = 0;
                        do
                        {
                            res.Add(multi.Read(types[index]).ToList());
                            index++;
                        } while (!multi.IsConsumed);
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Thực thi commandText trả về nhiều danh sách dữ liệu khác nhau
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="typeNames">tên các loại dữ liệu trả về</param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<object>>> QueryMultipleUsingCommandText(Guid tenantId, string appCode,
            List<string> typeNames, string commandText,
            Dictionary<string, object> dicParam, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            var listType=new List<Type>();
            for (int i = 0; i < typeNames?.Count; i++)
            {
                listType.Add(GetModelType(typeNames[i]));
            }
            Dictionary<string,List<object>>res=new Dictionary<string, List<object>>();
            var results = await QueryMultipleUsingCommandText(tenantId, appCode, listType, commandText,
                dicParam, transaction, connection);
            for (int i = 0; i < typeNames?.Count; i++)
            {
                res.Add(typeNames[i], results[i]);
            }

            return res;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về nhiều danh sách các dữ liệu khác nhau
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="typeNames">tên các loại dữ liệu trả về</param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, List<object>>> QueryMultipleUsingStoredProcedure(Guid tenantId, string appCode,
            List<string> typeNames, string storedProcedureName,
            object param = null, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            var listType=new List<Type>();
            for (int i = 0; i < typeNames?.Count; i++)
            {
                listType.Add(GetModelType(typeNames[i]));
            }
            Dictionary<string,List<object>>res=new Dictionary<string, List<object>>();
            var results = await QueryMultipleUsingStoredProcedure(tenantId, appCode, listType, storedProcedureName,
                param, transaction, connection);
            for (int i = 0; i < typeNames?.Count; i++)
            {
                res.Add(typeNames[i], results[i]);
            }

            return res;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về danh sách dữ liệu khác nhau (Không cần truyền types)
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public async Task<List<List<object>>> QueryMultipleUsingStoredProcedure(Guid tenantId, string appCode,
            string storedProcedureName, object param = null,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            List<List<object>>res = new List<List<object>>();
            var cd = BuildCommandDefinition(storedProcedureName, CommandType.StoredProcedure, param, transaction);
            var con = (transaction != null ? transaction.Connection : connection);
            if (con != null)
            {
                using (var multi= con.QueryMultiple(cd))
                {
                    int index = 0;
                    do
                    {
                        res.Add(multi.Read<dynamic>().ToList());
                        index++;
                    } while (!multi.IsConsumed);
                }
            }
            else
            {
                using (var cnn= GetConnection(tenantId,appCode))
                {
                    using (var multi= cnn.QueryMultiple(cd))
                    {
                        int index = 0;
                        do
                        {
                            res.Add(multi.Read<dynamic>().ToList());
                            index++;
                        } while (!multi.IsConsumed);
                    }
                }
            }

            return res;
        }

        #endregion

        

        #region Utility Method
        private CommandDefinition BuildCommandDefinition(string commandText, CommandType commandType, object param, IDbTransaction transaction)
        {
            if (commandType == CommandType.Text && !_allowRunCommandWithoutParameter &&
                (param == null || param.GetType() != typeof(Dictionary<string, object>) ||
                 ((Dictionary<string, object>) param).Count == 0))
            {
                throw new Exception(" DEV: Executing CommandText must have at least 1 parameter. ");
            }

            var commandDefinition = new CommandDefinition(commandText, commandType: commandType, parameters: param,
                transaction: transaction);
            return commandDefinition;
        }

        /// <summary>
        /// Sinh câu lệnh xoá entity
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="tenantId"></param>
        /// <param name="model"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="Exception"></exception>
        private void GenerateDelete<T>(ref string commandText, ref Dictionary<string, object> dicParam, Guid tenantId,
            T model = null) where T : BaseModel
        {
            string tableName = typeof(T).GetTableNameOnly();
            commandText = $"DELETE FROM `{SecureUtil.SafeSqlLiternalForObjectName(tableName)}`";
            ProcessConditionTenantId(ref commandText, ref dicParam, tenantId);
            if (model != null)
            {
                object id = model.GetPrimaryKey();
                if (id != null)
                {
                    commandText = commandText + $" AND {typeof(T).GetPrimaryKeyFieldName()} = @IDValue";
                    dicParam.Add("@IDValue",id);
                }
                else
                {
                    throw new Exception(" DELETE without ID");
                }
            }
            else
            {
                throw new Exception("DELETE without ID");
            }
        }
        private void GenerateDeleteByFieldNameAndValue(ref string commandText, ref Dictionary<string, object> dicParam, Guid tenantId, string tableName, string fieldName, object fieldValue)
        {
            commandText = $" DELETE FROM `{SecureUtil.SafeSqlLiternalForObjectName(tableName)}`";
            ProcessConditionTenantId(ref commandText, ref dicParam, tenantId);
            if (fieldValue != null)
            {
                commandText = commandText +
                              $" AND `{SecureUtil.SafeSqlLiternalForObjectName(fieldName)}` = @ForeignKeyValue";
                dicParam.Add("@ForeignKeyValue",fieldValue);
            }
            else
            {
                throw new Exception("DELETE without ID");
            }
        }
        
        /// <summary>
        /// Tạo query update 1 trường theo ID
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="fieldUpdate"></param>
        /// <param name="tenantId"></param>
        private void GenerateUpdateByFieldNameAndValue(ref string commandText, ref Dictionary<string, object> dicParam, FieldUpdate fieldUpdate,Guid tenantId)
        {
            var fieldUpdates=new List<string>();
            foreach (var item in fieldUpdate.FieldNameAndValue)
            {
                fieldUpdates.Add($"`{SecureUtil.SafeSqlLiternalForObjectName(item.Key)}` = @{item.Key}");
                dicParam.Add($"@{item.Key}",item.Value);
            }

            var baseModel = (BaseModel) Activator.CreateInstance(this.GetModelType(fieldUpdate.ModelName));
            commandText =
                $"UPDATE `{SecureUtil.SafeSqlLiternalForObjectName(baseModel.GetTableNameOnly())}` SET {string.Join(" , ", fieldUpdates)} ";
            ProcessConditionTenantId(ref commandText, ref dicParam, tenantId);
            commandText = commandText +
                          $" AND `{SecureUtil.SafeSqlLiternalForObjectName(fieldUpdate.FieldKey)}` = @KeyValue";
            dicParam.Add("@KeyValue",fieldUpdate.ValueKey);
        }
        private void GenerateSelectById(ref string commandText, ref Dictionary<string, object> dicParam, Guid tenantId,
            string typeName, string id)
        {
            GenerateSelectById(ref commandText, ref dicParam, tenantId, GetModelType(typeName), id);
        }

        /// <summary>
        /// get model type theo tên
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Type GetModelType(string typeName)
        {
            Type type = null;
            type = Type.GetType(string.Format(_modeNamespace, typeName));
            if (type == null)
            {
                throw new Exception($"Type [{typeName}] not found. ");
            }

            return type;
        }

        private void GenerateSelectById<T>(ref string commandText, ref Dictionary<string, object> dicParam,
            Guid tenantId, string id) where T : BaseModel
        {
            GenerateSelectById(ref commandText, ref dicParam, tenantId, typeof(T), id);
        }

        private void GenerateSelectById(ref string commandText, ref Dictionary<string, object> dicParam, Guid tenantId,
            Type type, string id)
        {
            commandText =
                $"SELECT t.* FROM '{SecureUtil.SafeSqlLiternalForObjectName(type.GetViewOrTableName())}' AS t";
            ProcessConditionTenantId(ref commandText, ref dicParam, tenantId);
            var idValue = GetSqlValue(id);
            if (id != null)
            {
                commandText = commandText + $" AND {type.GetPrimaryKeyFieldName()} = @IDValue";
                dicParam.Add("@IDValue", idValue);
            }
            else
            {
                throw new Exception("SelectByID without ID");
            }
        }

        /// <summary>
        /// sinh giá trị where dựa vào kiểu dữ liệu parameter
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object GetSqlValue(string value)
        {
            Guid guidValue = Guid.Empty;
            if (Guid.TryParse(value, out guidValue))
            {
                return guidValue;
            }
            else
            {
                long longValue = 0;
                if (long.TryParse(value, out longValue))
                {
                    return longValue;
                }
                else
                {
                    return value;
                }
            }
        }

        /// <summary>
        /// Tự động ghép điều kiện tenantId
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="tenantId"></param>
        /// <param name="isFirstCondition"></param>
        /// <param name="alias"></param>
        private void ProcessConditionTenantId(ref string commandText, ref Dictionary<string, object> dicParam,
            Guid tenantId, bool isFirstCondition = true, string alias = "")
        {
            if (tenantId != Guid.Empty)
            {
                if (_allowRunCommandWithoutTenantIdCondition)
                {
                    commandText += (isFirstCondition ? " WHERE 1 = 1" : " ");
                }
            }
            else
            {
                var appendCondition = isFirstCondition ? "WHERE" : "AND";
                var appendAlias = !string.IsNullOrWhiteSpace(alias) ? (alias + ".") : string.Empty;
                commandText += $"{appendCondition} {appendAlias}TenantId = @TenantId";
                dicParam.AddOrUpdate("@TenantId", tenantId);
            }
        }

        private bool IsExportData()
        {
            var isExportDataHeader = _httpContext.HttpContext?.Request?.Headers[Keys.IsExportData];
            if (isExportDataHeader.HasValue && isExportDataHeader.Value.ToString()
                .Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sinh câu lệnh select * từ model 
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="tenantId"></param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T"></typeparam>
        private void GenerateSelectAll<T>(ref string commandText, ref Dictionary<string, object> dicParam,
            Guid tenantId, string viewOrTableName = "") where T : BaseModel
        {
            GenerateSelectAll(ref commandText, ref dicParam, typeof(T), tenantId, viewOrTableName = "");
        }

        /// <summary>
        /// select * from entity
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="typeModel"></param>
        /// <param name="tenantId"></param>
        /// <param name="viewOrTableName"></param>
        private void GenerateSelectAll(ref string commandText, ref Dictionary<string, object> dicParam, Type typeModel,
            Guid tenantId, string viewOrTableName = "")
        {
            if (string.IsNullOrEmpty(viewOrTableName))
            {
                viewOrTableName = typeModel.GetViewOrTableName();
            }

            if (!string.IsNullOrEmpty(viewOrTableName))
            {
                commandText = $"SELECT T.* FROM '{SecureUtil.SafeSqlLiternalForObjectName(viewOrTableName)}' AS T";
                ProcessConditionTenantId(ref commandText, ref dicParam, tenantId);
            }
        }

        /// <summary>
        /// Sinh câu lệnh where phân trang
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        private void GeneratePaging(ref string commandText, int pageIndex, int pageSize, string sort)
        {
            List<GridSortItem> sortItems = new List<GridSortItem>();
            if (!string.IsNullOrWhiteSpace(sort))
            {
                sortItems = Converter.Deserialize<List<GridSortItem>>(sort);
            }

            GeneratePaging(ref commandText, pageIndex, pageSize, sortItems);
        }

        /// <summary>
        /// Sinh câu lệnh where phân trang
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sort"></param>
        private void GeneratePaging(ref string commandText, int pageIndex, int pageSize, List<GridSortItem> sorts)
        {
            if (sorts?.Count > 0)
            {
                commandText += " ORDER BY ";
                foreach (var sortItem in sorts)
                {
                    if (SecureUtil.DetectSqlInjection(sortItem.Selector))
                    {
                        throw new FormatException();
                    }

                    commandText +=
                        $"`{SecureUtil.SafeSqlLiternalForObjectName(sortItem.Selector)}` {(sortItem.Desc ? "DESC" : "ASC")}, ";
                }

                commandText = commandText.Substring(0, commandText.Length - 2);
            }
            else
            {
                commandText += " ORDER BY ModifiedDate DESC";
            }

            if (pageSize >= 0 && !IsExportData()) // pageSize < 0 hoặc lấy dữ liệu đề xuất khẩu thì getall
            {
                if (pageSize > DatabaseConstant.MaxReturnRecord)
                {
                    pageSize = DatabaseConstant.MaxReturnRecord;
                }

                if (pageIndex <= 0)
                {
                    pageIndex = 1;
                }

                if (pageIndex > DatabaseConstant.MaxPageIndex)
                {
                    pageIndex = DatabaseConstant.MaxPageIndex;
                }

                commandText += $" LIMIT {pageSize} OFFSET {(pageIndex - 1) * pageSize} ";
            }
        }

        /// <summary>
        /// Decode query dạng base64
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string DecodeBase64Param(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return "";
            }

            filter = Converter.Base64Decode(filter);
            filter = Converter.UrlDecode(filter).Replace("\\", "");
            return filter;
        }

        /// <summary>
        /// Sinh câu lệnh select từ entity
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="tenantId"></param>
        /// <param name="columns"></param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T"></typeparam>
        private void GenerateSelectByColumn<T>(ref string commandText, ref Dictionary<string, object> dicParam,
            Guid tenantId, string columns, string viewOrTableName = "") where T : BaseModel
        {
            GenerateSelectByColumn(ref commandText, ref dicParam, typeof(T), tenantId, columns, viewOrTableName);
        }

        /// <summary>
        /// sinh câu lệnh select từ entity
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="typeModel"></param>
        /// <param name="tenantId"></param>
        /// <param name="columns"></param>
        /// <param name="viewOrTableName"></param>
        private void GenerateSelectByColumn(ref string commandText, ref Dictionary<string, object> dicParam,
            Type typeModel, Guid tenantId, string columns, string viewOrTableName = "")
        {
            if (string.IsNullOrEmpty(viewOrTableName))
            {
                viewOrTableName = typeModel.GetViewOrTableName();
            }

            if (!string.IsNullOrEmpty(viewOrTableName))
            {
                commandText =
                    $"SELECT {SecureUtil.SafeSqlLiteralForColumnsName(columns)} FROM `{SecureUtil.SafeSqlLiternalForObjectName(viewOrTableName)}`";
                ProcessConditionTenantId(ref commandText, ref dicParam, tenantId);
            }
        }
        /// <summary>
        /// Sinh lệnh select detail theo Type Master
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="tenantId"></param>
        /// <param name="masterKey"></param>
        /// <param name="s"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void GenerateSelectDetailByTypeMaster(ref string commandText, ref Dictionary<string, object> dicParam, Guid tenantId, Type masterType, string masterKey)
        {
            var baseModel = (BaseModel) Activator.CreateInstance(masterType);
            if (baseModel.ModelDetailConfigs != null)
            {
                foreach (var modelDetailConfig in baseModel.ModelDetailConfigs)
                {
                    if (!string.IsNullOrWhiteSpace(modelDetailConfig.PropertyOnMasterModel))
                    {
                        commandText +=
                            $"SELECT A.* FROM `{SecureUtil.SafeSqlLiternalForObjectName(modelDetailConfig.DetailTableName)}` AS A";
                        ProcessConditionTenantId(ref commandText, ref dicParam, tenantId);
                        commandText += $" AND {modelDetailConfig.ForeignKeyName} = @ForeignKeyId;";
                    }
                }
                dicParam.Add("@ForeignKeyId",GetSqlValue(masterKey));
            }
        }
        #endregion
    }
}