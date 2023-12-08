using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Contant;
using Core.Database;
using Core.Database.SQLHelper;
using Core.Enumeration;
using Core.Http;
using Core.Interface;
using Core.Model;
using Core.Model.Platform;
using Core.Services;
using Core.Ultitily;
using Core.Utility;
 
using BaseModel = Core.Model.BaseModel;

namespace Core.BL
{
    public  class BaseBL : IBaseBL
    {
        public virtual string ApplicationCode => string.Empty;
        protected readonly IAuthService _authService;
        protected readonly ICacheService _cacheService;
        protected readonly IMemoryCacheService _memoryCacheService;
        protected readonly IConfigService _configService;
        protected readonly ILogService _logService;
        // protected readonly IOptionService _optionService;
        protected readonly IDatabaseService _databaseService;
        protected readonly IHttpService _httpService;
        //protected readonly IPushNotificationService _pushNotificationService;
        //protected readonly INotificationService _notificationService;
        //protected readonly INotificationCenterService _notificationCenterService;
        protected readonly ISessionBL _sessionBl;

        protected readonly Guid _userId;
        protected readonly string _userName;
        protected readonly string _fullName;
        protected Guid _tenantId;
        protected string _tenantCode;

        private readonly string _procNotificationInsert = "Proc_Notification_Insert";

        private readonly string[] _formatDateTimes =
        {
            "dd/MM/yyyy", "dd/M/yyyy", "d/MM/yyyy", "d/M/yyyy", "M/d/yyyy", "MM/d/yyyy", "M/d/yyyy", "MM/dd/yyyy",
            "M/dd/yyyy"
        };

        public Guid TenantId
        {
            get { return _tenantId; }
            set { _tenantId = value; }
        }

        public BaseBL(CoreServiceCollection serviceCollection)
        {
            _authService = serviceCollection.AuthService();
            //_cacheService = serviceCollection.CacheService();
            _memoryCacheService = serviceCollection.MemoryCacheService();
            _configService = serviceCollection.ConfigService();
            _logService = serviceCollection.LogService();
            // _optionService = serviceCollection.OptionService;
            _databaseService = serviceCollection.DatabaseService();
            _httpService = serviceCollection.HttpService();
            //_pushNotificationService = serviceCollection.PushNotificationService;
            //_notificationService = serviceCollection.NotificationService;
            //_notificationCenterService = serviceCollection.NotificationCenterService;
            //_sessionBl = serviceCollection.SessionBl;
            _userId = _authService.GetUserId();
            _userName = _authService.GetUserName();
            _fullName = _authService.GetFullName();
            _tenantId = _authService.GetTenantId();
            _tenantCode = _authService.GetTenantCode();
        }

        public string SubSystemCode { get; set; }

        public string GetMisaCodeByOrganizationUnitId(Guid organizationUnitId)
        {
            throw new NotImplementedException();
        }

        public List<SC_ListRoleByApp> GetListRoles()
        {
            throw new NotImplementedException();
        }

        public void RunCommandWithoutParameter(Action action)
        {
            _databaseService.RunCommandWithoutParameter(action);
        }

        public void RunCommandWithoutTenantIdCondition(Action action)
        {
            _databaseService.RunCommandWithoutTenantIdCondition(action);
        }

        public Dictionary<string, object> GetAllPermissionByApp()
        {
            throw new NotImplementedException();
        }

        public bool CheckPermission(string subSystemCode, string[] permissionCodes, Guid? organizationUnitId = null,
            bool isAndPermission = true)
        {
            throw new NotImplementedException();
        }

        public List<ValidateResult> CheckPermission(BaseModel baseModel, string[] action)
        {
            throw new NotImplementedException();
        }

        public string[] GetPermissionSaveData(BaseModel baseModel)
        {
            throw new NotImplementedException();
        }

        public string[] GetPermissionDeleteData(BaseModel baseModel)
        {
            throw new NotImplementedException();
        }

        public string[] GetPermissionView(BaseModel baseModel)
        {
            throw new NotImplementedException();
        }

        #region GetMethod

        public T GetById<T>(string id) where T : BaseModel
        {
            return _databaseService.GetById<T>(_tenantId, this.ApplicationCode, id).Result;
        }

        public bool IsAdmin()
        {
            return false;
        }

        public dynamic GetById(string typeName, string id)
        {
            return _databaseService.GetById(_tenantId, this.ApplicationCode, typeName: typeName, id).Result;
        }

        public dynamic GetById(Type modelType, string id)
        {
            return _databaseService.GetById(_tenantId, this.ApplicationCode, modelType, id).Result;
        }

        public dynamic GetFormData(Type modelType, string id)
        {
            var baseModel = (BaseModel) Activator.CreateInstance(modelType);
            var data = _databaseService.GetById(_tenantId, this.ApplicationCode, modelType, id).Result;
            if (data != null)
            {
                baseModel = (BaseModel) data;
                if (baseModel.ModelDetailConfigs != null)
                {
                    var detailTypes = new List<string>();
                    var detailProperType = new Dictionary<string, string>();
                    foreach (ModelDetailConfig detailConfig in baseModel.ModelDetailConfigs.Where(c =>
                        !string.IsNullOrWhiteSpace(c.PropertyOnMasterModel)))
                    {
                        var listType = base.GetType().GetPropertyType(detailConfig.PropertyOnMasterModel);
                        detailProperType.AddOrUpdate(listType.GetGenericArguments().Single().Name,
                            detailConfig.PropertyOnMasterModel);
                    }

                    var dataDetail = this.GetListDetailByMaster(id, modelType);
                    foreach (var item in dataDetail)
                    {
                        if (detailProperType.ContainsKey(item.Key))
                        {
                            baseModel.SetValue(detailProperType[item.Key],
                                Converter.DeserializeObject(Converter.Serialize(item.Value),
                                    baseModel.GetType().GetPropertyType(detailProperType[item.Key])));
                        }
                    }
                }
            }

            AfterGetFormData(baseModel);
            return baseModel;
        }

        /// <summary>
        /// Xử lí sau khi get form data về
        /// </summary>
        /// <param name="baseModel"></param>
        public virtual void AfterGetFormData(BaseModel baseModel)
        {
        }

        public List<T> GetByListId<T>(string ids) where T : BaseModel
        {
            return _databaseService.GetByListId<T>(_tenantId, this.ApplicationCode, ids).Result;
        }

        public object GetAll<T>(string filter, string sort, string customFilter = "",
            WhereParameter whereParameter = null,
            string columns = "", string viewOrTableName = "") where T : BaseModel
        {
            return _databaseService.GetAll<T>(_tenantId, this.ApplicationCode, filter, sort, customFilter,
                whereParameter, columns, viewOrTableName).Result;
        }

        public object GetAll(string typeName, string filter, string sort, string customFilter = "",
            WhereParameter whereParameter = null, string columns = "", string viewOrTableName = "")
        {
            return _databaseService.GetAll(_tenantId, this.ApplicationCode, typeName, filter, sort, customFilter,
                whereParameter, columns, viewOrTableName).Result;
        }

        public PagingResponse GetPagingUsingCommandText<T>(int pageSize, int pageIndex, WhereParameter filter,
            List<GridSortItem> sorts,
            WhereParameter customFilter = null, WhereParameter fixedFilter = null, string columns = "",
            string viewOrTableName = "") where T : BaseModel
        {
            return _databaseService.GetPagingUsingCommandText<T>(_tenantId, this.ApplicationCode, pageSize, pageIndex,
                filter, sorts, customFilter, fixedFilter, columns, viewOrTableName).Result;
        }

        public PagingResponse GetPagingUsingCommanText<T>(int pageSize, int pageIndex, string filter, string sort,
            string customFilter,
            WhereParameter fixedFilter = null, string columns = "", string viewOrTableName = "") where T : BaseModel
        {
            return _databaseService.GetPagingUsingCommandText<T>(_tenantId, this.ApplicationCode, pageSize, pageIndex,
                filter, sort,
                customFilter, fixedFilter, columns, viewOrTableName).Result;
        }

        public PagingResponse GetPagingUsingCommanText(Type typeModel, int pageSize, int pageIndex, string filter,
            string sort,
            string customFilter, WhereParameter fixedFilter = null, string columns = "", string viewOrTableName = "")
        {
            return _databaseService.GetPagingUsingCommandText(typeModel, _tenantId, this.ApplicationCode, pageSize,
                pageIndex, filter, sort, customFilter, fixedFilter, columns, viewOrTableName).Result;
        }

        public PagingResponse GetPagingUsingStoredProcedure<T>(int pageSize, int pageIndex, string filter, string sort,
            string customFilter, string storedProcedureName = "", Dictionary<string, object> param = null)
            where T : BaseModel
        {
            return _databaseService.GetPagingUsingStoredProcedure<T>(_tenantId, this.ApplicationCode, pageSize,
                pageIndex, filter, sort, customFilter, storedProcedureName, param).Result;
        }

        public PagingResponse GetPagingUsingStoredProcedure(Type typeModel, int pageSize, int pageIndex, string filter,
            string sort,
            string customFilter, string storedProcedure = "", Dictionary<string, object> param = null,
            WhereParameter fixedFilter = null)
        {
            return _databaseService.GetPagingUsingStoredProcedure(typeModel, _tenantId, this.ApplicationCode, pageSize,
                pageIndex, filter, sort, customFilter, storedProcedure, param, fixedFilter).Result;
        }

        public PagingResponse GetPaging(Type typeModel, PagingRequest pagingRequest, string storedProcedureName = "",
            Dictionary<string, object> param = null)
        {
            WhereParameter whereParameter = BuildWhereParameter(typeModel, pagingRequest);
            if (pagingRequest.UseSp)
            {
                return _databaseService.GetPagingUsingStoredProcedure(typeModel, _tenantId, this.ApplicationCode,
                    pagingRequest.PageSize, pagingRequest.PageIndex, pagingRequest.Filter, pagingRequest.Sort,
                    pagingRequest.CustomFilter, storedProcedureName, param, whereParameter).Result;
            }
            else
            {
                return _databaseService.GetPagingUsingCommandText(typeModel, _tenantId, this.ApplicationCode,
                    pagingRequest.PageSize, pagingRequest.PageIndex, pagingRequest.Filter, pagingRequest.Sort,
                    pagingRequest.CustomFilter, whereParameter).Result;
            }
        }

        /// <summary>
        /// Build câu lệnh search value cho danh sách cột
        /// 
        /// </summary>
        /// <param name="typeModel"></param>
        /// <param name="pagingRequest"></param>
        /// <returns></returns>
        public WhereParameter BuildWhereParameter(Type typeModel, PagingRequest pagingRequest)
        {
            WhereParameter whereParameter = null;
            if (pagingRequest.QuickSearch != null && !string.IsNullOrWhiteSpace(pagingRequest.QuickSearch.SearchValue))
            {
                //Build weher paramter khi co quicksearch
                List<string> quickSearchs = new List<string>();
                var dicParam = new Dictionary<string, object>();
                dicParam.Add("@SearchValue", pagingRequest.QuickSearch.SearchValue);
                for (int i = 0; i < pagingRequest.QuickSearch.Columns.Length; i++)
                {
                    var columnName = pagingRequest.QuickSearch.Columns[i];
                    //kiểm tra dữ liệu
                    //datetime: parse về DateTime kiểu so sánh bettween
                    //các kiểu còn lại thì so sánh %
                    if (typeModel.GetPropertyType(columnName) != null)
                    {
                        var type = typeModel.GetPropertyType(columnName);
                        if (Nullable.GetUnderlyingType(type) == typeof(DateTime))
                        {
                            DateTime d = DateTime.Now;
                            if (DateTime.TryParseExact(pagingRequest.QuickSearch.SearchValue, _formatDateTimes, null,
                                DateTimeStyles.None, out d))
                            {
                                quickSearchs.Add(
                                    $"({SecureUtil.SafeSqlLiternalForObjectName(columnName)} BETWEEN @{columnName}_FromDate AND @{columnName}_ToDate)");
                                dicParam.AddOrUpdate($"@{columnName}_FromDate", d.Date);
                                dicParam.AddOrUpdate($"@{columnName}_ToDate", d.AddDays(1).Date.AddMilliseconds(-1));
                            }
                        }
                        else
                        {
                            quickSearchs.Add(
                                $"{SecureUtil.SafeSqlLiternalForObjectName(columnName)} LIKE CONCAT('%',@SearchValue,'%')");
                        }
                    }
                }

                whereParameter = new WhereParameter($"({string.Join(" OR ", quickSearchs)}", dicParam);
            }

            return whereParameter;
        }

        /// <summary>
        /// Lấy danh sách object detail theo master id
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="masterKey"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public dynamic GetDetailByMaster(string modelType, string masterKey, string filter)
        {
            return _databaseService.GetDetailByMaster(_tenantId, this.ApplicationCode, modelType, masterKey, filter)
                .Result;
        }

        /// <summary>
        /// Lấy danh sách detail của Type master
        /// </summary>
        /// <param name="masterKey">giá trị khoá chính của master<giá/param>
        /// <param name="masterType">loại của master</param>
        /// <returns></returns>
        public Dictionary<string, List<object>> GetListDetailByMaster(string masterKey, Type masterType)
        {
            return _databaseService.GetListDetailByMaster(_tenantId, this.ApplicationCode, masterType, masterKey)
                .Result;
        }

        /// <summary>
        /// Lấy danh sách các loại detail theo id của master
        /// </summary>
        /// <param name="masterKey"></param>
        /// <param name="detailType">các loại detail model</param>
        /// <returns></returns>
        public Dictionary<string, List<object>> GetListDetailByMaster(string masterKey, List<string> detailType)
        {
            return _databaseService.GetListDetailByMaster(_tenantId, this.ApplicationCode, masterKey, detailType)
                .Result;
        }

        /// <summary>
        /// Lấy connection của từng app từng cty
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetDbConnection()
        {
            return _databaseService.GetConnection(_tenantId, this.ApplicationCode);
        }

        #endregion

        #region Execute Method

        /// <summary>
        /// Thực thi một commandText trả về thành công/thất bại
        /// </summary>
        /// <param name="commandText">command text cần thực thi</param>
        /// <param name="dicParam">từ điểm các tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public bool ExecuteUsingCommandText(string commandText, Dictionary<string, object> dicParam,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return _databaseService.ExecuteUsingCommandText(_tenantId, this.ApplicationCode, commandText, dicParam,
                transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về 1 giá trị
        /// </summary>
        /// <param name="storedProcedureName">tên stored</param>
        /// <param name="param">object/Từ điển các tham số của StoredProcedure</param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool ExecuteUsingStoredProcedure(string storedProcedureName, object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return _databaseService.ExecuteUsingStoredProcedure(_tenantId, this.ApplicationCode, storedProcedureName,
                param, transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một commandText trả về thành công/thất bại
        /// </summary>
        /// <param name="commandText">command text cần thực thi</param>
        /// <param name="dicParam">từ điểm các tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T">loại model</typeparam>
        /// <returns></returns>
        public T ExecuteScalarUsingCommandText<T>(string commandText, Dictionary<string, object> dicParam = null,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return _databaseService.ExecuteScalarUsingCommandText<T>(_tenantId, this.ApplicationCode, commandText,
                dicParam, transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về 1 giá trị
        /// </summary>
        /// <param name="storedProcedureName">tên stored</param>
        /// <param name="param">object/Từ điển các tham số của StoredProcedure</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ExecuteScalarUsingStoredProcedure<T>(string storedProcedureName, object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return _databaseService.ExecuteScalarUsingStoredProcedure<T>(_tenantId, this.ApplicationCode,
                storedProcedureName,
                param, transaction, connection).Result;
        }

        /// <summary>
        /// Xoá 1 model
        /// </summary>
        /// <param name="model">object model</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T">loại model</typeparam>
        /// <returns></returns>
        public bool ExecuteDelete<T>(T model, IDbTransaction transaction = null, IDbConnection connection = null)
            where T : BaseModel
        {
            return _databaseService.ExecuteDelete<T>(_tenantId, this.ApplicationCode, model, transaction, connection)
                .Result;
        }

        /// <summary>
        /// Xoá 1 model
        /// </summary>
        /// <param name="model">object model</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public bool ExecuteDelete(BaseModel model, IDbTransaction transaction = null, IDbConnection connection = null)
        {
            return _databaseService.ExecuteDelete(_tenantId, this.ApplicationCode, model, transaction, connection)
                .Result;
        }

        /// <summary>
        /// Xoá bản ghi theo trường
        /// </summary>
        /// <param name="tableName">tên bảng</param>
        /// <param name="fieldName">tên trường làm điều kiện xoá</param>
        /// <param name="fieldValue">giá trị của trường</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public bool ExecuteDeleteByFieldNameAndValue(string tableName, string fieldName, object fieldValue,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            return _databaseService.ExecuteDeleteByFieldNameAndValue(_tenantId, ApplicationCode, tableName, fieldName,
                fieldValue, transaction, connection).Result;
        }

        /// <summary>
        /// Xoá bản ghi theo trường
        /// </summary>
        /// <param name="modelType">loại model</param>
        /// <param name="fieldName">tên trường làm điều kiện xoá</param>
        /// <param name="fieldValue">giá trị của trường</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public bool ExecuteDeleteByFieldNameAndValue(Type modelType, string fieldName, object fieldValue,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            return _databaseService.ExecuteDeleteByFieldNameAndValue(_tenantId, this.ApplicationCode, modelType,
                fieldName, fieldValue, transaction, connection).Result;
        }

        /// <summary>
        /// Update thông tin 1 trường theo khoá
        /// </summary>
        /// <param name="fieldUpdate">Data field update</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public ServiceResponse ExecuteUpdateField(FieldUpdate fieldUpdate, IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            var baseModel = this.GetById(fieldUpdate.ModelName, fieldUpdate.ValueKey.ToString());
            if (baseModel != null)
            {
                fieldUpdate.DataModel = baseModel;
            }

            var res = this._databaseService.ExecuteUpdateField(this._tenantId, this.ApplicationCode, fieldUpdate,
                transaction, connection).Result;
            serviceResponse.Success = res;
            //Thêm hàm xử lí sau khi update field thành công
            if (res)
            {
                AfterExecuteUpdateField(serviceResponse, fieldUpdate);
            }

            return serviceResponse;
        }

        /// <summary>
        /// Xử lí sau khi update field thành công
        /// </summary>
        /// <param name="serviceResponse"></param>
        /// <param name="fieldUpdate"></param>
        public virtual void AfterExecuteUpdateField(ServiceResponse serviceResponse, FieldUpdate fieldUpdate)
        {
            //TODO: Override xử lí nghiệp vụ
        }

        #endregion

        #region Query Method

        /// <summary>
        /// Thực thi một command text trả về danh sách
        /// </summary>
        /// <param name="commandText">commandText cần thực thi</param>
        /// <param name="dicParam">Từ điể tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T">loại model</typeparam>
        /// <returns></returns>
        public List<T> QueryUsingCommandText<T>(string commandText, Dictionary<string, object> dicParam,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return _databaseService.QueryUsingCommandText<T>(_tenantId, this.ApplicationCode, commandText, dicParam,
                transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một command text trả về danh sách dynamic
        /// </summary>
        /// <param name="commandText">commandText cần thực thi</param>
        /// <param name="dicParam">Từ điể tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public IEnumerable<dynamic> QueryUsingCommandText(string commandText, Dictionary<string, object> dicParam,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return _databaseService.QueryUsingCommandText(_tenantId, this.ApplicationCode, commandText, dicParam,
                transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về danh sách
        /// </summary>
        /// <param name="storedProcedureName">Tên storedProcedure</param>
        /// <param name="param">Object/Từ điển chứa tham số stored</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T">loại model</typeparam>
        /// <returns></returns>
        public List<T> QueryUsingStoredProcedure<T>(string storedProcedureName, object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return _databaseService.QueryUsingStoredProcedure<T>(_tenantId, this.ApplicationCode, storedProcedureName,
                param, transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về danh sách dynamic
        /// </summary>
        /// <param name="storedProcedureName">Tên storedProcedure</param>
        /// <param name="param">Object/Từ điển chứa tham số stored</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public IEnumerable<dynamic> QueryUsingStoredProcedure(string storedProcedureName, object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return _databaseService.QueryUsingStoredProcedure(_tenantId, this.ApplicationCode, storedProcedureName,
                param, transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một command trả về nhiều danh sách dữ liệu khác nhau
        /// </summary>
        /// <param name="types">các loại dữ liệu trả về</param>
        /// <param name="commandText">commandText cần thực thi</param>
        /// <param name="dicParam">Từ điể tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public List<List<object>> QueryMultipleUsingCommandText(List<Type> types, string commandText,
            Dictionary<string, object> dicParam,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            return _databaseService.QueryMultipleUsingCommandText(_tenantId, this.ApplicationCode, types, commandText,
                dicParam, transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về nhiều danh sách dữ liệu khác nhau
        /// </summary>
        /// <param name="types">các loại dữ liệu trả về</param>
        /// <param name="storedProcedureName">Tên stored cần thực thi</param>
        /// <param name="param"></param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public List<List<object>> QueryMultipleUsingStoredProcedure(List<Type> types, string storedProcedureName,
            object param,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            return _databaseService.QueryMultipleUsingStoredProcedure(_tenantId, this.ApplicationCode,
                storedProcedureName, param, transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một command trả về nhiều danh sách dữ liệu khác nhau
        /// </summary>
        /// <param name="typeNames">các loại dữ liệu trả về</param>
        /// <param name="commandText">commandText cần thực thi</param>
        /// <param name="dicParam">Từ điể tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public Dictionary<string, List<object>> QueryMultipleUsingCommandText(List<string> typeNames,
            string commandText, Dictionary<string, object> dicParam,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            return _databaseService.QueryMultipleUsingCommandText(_tenantId, this.ApplicationCode, typeNames,
                commandText, dicParam, transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về nhiều danh sách dữ liệu khác nhau
        /// </summary>
        /// <param name="typeNames">các loại dữ liệu trả về</param>
        /// <param name="storedProcedureName">Tên stored cần thực thi</param>
        /// <param name="param"></param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        /// <returns></returns>
        public Dictionary<string, List<object>> QueryMultipleUsingStoredProcedure(List<string> typeNames,
            string storedProcedureName, object param,
            IDbTransaction transaction = null, IDbConnection connection = null)
        {
            return _databaseService.QueryMultipleUsingStoredProcedure(_tenantId, this.ApplicationCode, typeNames,
                storedProcedureName, param, transaction, connection).Result;
        }

        /// <summary>
        /// Thực thi một storedProcedure trả về nhiều danh sách dữ liệu khác nhau
        /// </summary>
        /// <param name="storedProcedureName">Tên stored cần thực thi</param>
        /// <param name="param"></param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        public List<List<object>> QueryMultipleUsingStoredProcedure(string storedProcedureName, object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null)
        {
            return _databaseService
                .QueryMultipleUsingStoredProcedure(_tenantId, this.ApplicationCode, storedProcedureName, param,
                    transaction,
                    connection).Result;
        }

        #endregion

        #region Generate Clause Paging Method

        /// <summary>
        /// Sinh mệnh đề sort
        /// </summary>
        /// <param name="column">tên cột</param>
        /// <param name="desc">loại sắp xếp</param>
        /// <returns></returns>
        public string GenerateSortClause(string column, bool desc)
        {
            var sorts = new List<Dictionary<string, object>>();
            sorts.Add(new Dictionary<string, object>() {{"selector", column}, {"desc", desc}});
            var sort = Converter.UrlEncode(Converter.Serialize(sorts));
            return sort;
        }

        #endregion

        #region Common Function

        /// <summary>
        /// trả về câu query
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetQueryString(string key)
        {
            return Common.QueryMySql(this._configService.GetAppSetting(AppSettingsKey.QueryPath), key);
        }

        #endregion

        /// <summary>
        /// Method gọi các api ở mức server - server
        /// </summary>
        /// <param name="apiUrlKey"> Key của APIUrl (vd: ApiUrlKey.PlatformAPI)</param>
        /// <param name="apiPath">path đến method của controller (vd: Role/GetRole)</param>
        /// <param name="method">phương thức: GET/PUT/POST/DELETE</param>
        /// <param name="param">object cần truyền vào body của requet</param>
        /// <param name="authorizationToken"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected async Task<ServiceResponse> CallInternalApi(string apiUrlKey, string apiPath, HttpMethod method,
            object param = null, string authorizationToken = null, Dictionary<string, string> headers = null)
        {
            //_logService.LogTrace($"apiUrlKey: {apiUrlKey} {Environment.NewLine} " +
            //                     $"apiPath: {apiPath} {Environment.NewLine} " +
            //                     $"httpService.GetSession: {((BaseHttpClient) _httpService).GetSessionId()} {Environment.NewLine} " +
            //                     $"_authService.GetSessionID: {_authService.GetSessionId()} {Environment.NewLine} " +
            //                     $"_authService.GetToken: {_authService.GetToken()}"
            //);
            return await _httpService.CallInternalApi(apiUrlKey, apiPath, method, param, authorizationToken, headers);
        }

        #region SaveData

        public ServiceResponse SaveData(BaseModel baseModel)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            IDbTransaction transaction = null;
            IDbConnection connection = null;
            try
            {
                var validateResults = ValidateSaveData(baseModel);
                if (validateResults.Count > 0)
                {
                    //Kiểm tra lỗi có dc pass qua khong
                    /// -Nếu được pass thì tiếp tục
                    var validateResultPass = new List<ValidateResult>();
                    if (baseModel.PassWarningCode != null && baseModel.PassWarningCode.Length > 0)
                    {
                        validateResultPass =
                            validateResults.FindAll(x => baseModel.PassWarningCode.Contains(x.Code));
                    }

                    if (validateResultPass.Count != validateResults.Count)
                    {
                        serviceResponse.ValidateInfo.AddRange(validateResults);
                        serviceResponse.Success = false;
                        return serviceResponse;
                    }
                }

                // Xử lí trước khi cất
                this.BeforeSave(baseModel);
                connection = this.GetDbConnection();
                connection.Open();
                transaction = connection.BeginTransaction();

                bool result = this.DoSave(baseModel, transaction);
                //sau khi lưu thành công
                if (result)
                {
                    // Xử lí thêm nghiệp vụ sau khi lưu và còn transaction
                    AfterSave(baseModel, transaction);
                    //commit transaction
                    transaction.Commit();

                    //ghi log
                }
                else
                {
                    transaction.Rollback();
                    serviceResponse.Success = false;
                }
            }
            catch (Exception e)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }

                throw e;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            if (serviceResponse.Success)
            {
                AfterCommit(baseModel, serviceResponse);
            }

            return serviceResponse;
        }


        /// <summary>
        /// Validate dữ liệu trước khi cất
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual List<ValidateResult> ValidateSaveData(BaseModel baseModel)
        {
            // Check phân quyền
            List<ValidateResult> validateResults = new List<ValidateResult>();

            // Check bản ghi có đang bị chỉnh sửa không
            //lấy old Data và kiểm tra giá trị edit version
            if (baseModel.GetHasEditVersion() && baseModel.State == ModelState.Update && baseModel.EditVersion != null)
            {
                var commandText =
                    $"SELECT {DatabaseConstant.EditVersion} FROM {baseModel.GetTableNameOnly()} WHERE {baseModel.GetPrimaryKeyFieldName()} = @ID";
                var dic = new Dictionary<string, object>();
                var editVersion = ExecuteScalarUsingCommandText<DateTime>(commandText, dic);
                if (baseModel.EditVersion != editVersion)
                {
                    validateResults.Add(new ValidateResult()
                    {
                        Code = ErrorCode.VERSION_OLD,
                        ErrorMessage = "CoreResources.VERSION_OLD",
                        Id = baseModel.GetPrimaryKeyValue()
                    });
                }
            }

            if (validateResults.Count == 0)
            {
                validateResults = CheckDuplicateData(baseModel);
            }

            //TODO : từng nghiệp vụ riêng sẽ override lại hàm này để validate
            return validateResults;
        }

        /// <summary>
        /// Check dữ liệu xem có bị trùng với bản ghi nào trong hệ thống không
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual List<ValidateResult> CheckDuplicateData(BaseModel baseModel)
        {
            List<ValidateResult> validateResults = new List<ValidateResult>();
            // Kiểm tra có config bản khi trùng không
            var fieldUniques = baseModel.GetFieldUnique();
            if (!string.IsNullOrWhiteSpace(fieldUniques))
            {
                var dicFieldByValue = new Dictionary<string, object>();
                var arr = fieldUniques.Split(";");
                foreach (var fieldUnique in arr)
                {
                    var value = baseModel.GetValue<object>(fieldUnique);
                    if (value != null)
                    {
                        dicFieldByValue.AddOrUpdate(fieldUnique, value);
                    }
                }

                var res = this._databaseService.ExecuteCheckDuplicateDate(_tenantId, this.ApplicationCode,
                    baseModel.GetTableNameOnly(), baseModel.GetPrimaryKeyFieldName(), baseModel.GetPrimaryKeyValue(),
                    dicFieldByValue).Result;
                if (res)
                {
                    validateResults.Add(new ValidateResult()
                    {
                        Code = ErrorCode.DUPLICATEDATA,
                        ValidateType = ValidateType.Unique,
                        Id = baseModel.GetPrimaryKeyValue()
                    });
                }
            }

            return validateResults;
        }

        /// <summary>
        /// Chuẩn bị dữ liệu trước khi lưu
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        public virtual bool BeforeSave(BaseModel baseModel)
        {
            //TODO Ham de override lai khi can

            if (baseModel.State == ModelState.Insert || baseModel.State == ModelState.Duplicate)
            {
                baseModel.CreatedBy = _userName;
                baseModel.CreatedDate = DateTime.Now;
                if (baseModel.GetPrimaryKeyValue() == null)
                {
                    baseModel.SetAutoPrimaryKey();
                }
            }

            baseModel.ModifiedBy = _userName;
            baseModel.ModifiedDate = DateTime.Now;
            baseModel.TenantId = _tenantId;
            baseModel.EditVersion = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Thực hiện lưu data
        /// </summary>
        /// <param name="baseModel"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool DoSave(BaseModel baseModel, IDbTransaction transaction)
        {
            // Save Master
            string storeName = DatabaseConstant.Proc_Insert;
            object primaryKeyValue = baseModel.GetPrimaryKeyValue();
            if (baseModel.State == ModelState.Update)
            {
                storeName = DatabaseConstant.Proc_Update;
            }

            var dic = Converter.ConvertDatabaseParam(baseModel);
            if (baseModel.GetPrimaryKeyType() == typeof(int))
            {
                primaryKeyValue =
                    this.ExecuteScalarUsingStoredProcedure<int>(string.Format(storeName, baseModel.GetTableNameOnly()),
                        dic, transaction);
                if (baseModel.State == ModelState.Insert || baseModel.State == ModelState.Duplicate)
                {
                    baseModel.SetValueByAttribute(typeof(KeyAttribute), primaryKeyValue);
                }
            }
            else
            {
                this.ExecuteScalarUsingStoredProcedure<object>(string.Format(storeName, baseModel.GetTableNameOnly()),
                    dic, transaction);
            }

            //save detail
            if (baseModel.ModelDetailConfigs != null)
            {
                foreach (var detailConfig in baseModel.ModelDetailConfigs.Where(c =>
                    !string.IsNullOrWhiteSpace(c.PropertyOnMasterModel)))
                {
                    IList listDetailObject = baseModel.GetValue<IList>(detailConfig.PropertyOnMasterModel);
                    if (listDetailObject != null)
                    {
                        foreach (BaseModel detail in listDetailObject)
                        {
                            if (detail.State == ModelState.Insert || detail.State == ModelState.Duplicate ||
                                detail.State == ModelState.Update)
                            {
                                //Gán lại khoá chính cho các detail
                                detail.SetValue(detailConfig.ForeignKeyName, baseModel.GetPrimaryKeyValue());
                                if (detail.State == ModelState.Insert || detail.State == ModelState.Duplicate)
                                {
                                    detail.CreatedBy = _userName;
                                    detail.CreatedDate = DateTime.Now;
                                    if (detail.GetPrimaryKeyValue() == null)
                                    {
                                        detail.SetAutoPrimaryKey();
                                    }
                                }

                                detail.ModifiedBy = _userName;
                                detail.ModifiedDate = DateTime.Now;
                                detail.TenantId = _tenantId;
                                DoSave(detail, transaction);
                            }
                            else if (detail.State == ModelState.Delete)
                            {
                                DoDelete(detail, transaction);
                            }
                        }
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Xử lí sau khi lưu giá trị, còn transaction
        /// </summary>
        /// <param name="baseModel"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private bool AfterSave(BaseModel baseModel, IDbTransaction transaction)
        {
            //TODO: Xử lí sau khi vẫn còn transaction
            return true;
        }

        /// <summary>
        /// Xử lí sau khi lưu thành công và không còn transaction
        /// </summary>
        /// <param name="baseModel"></param>
        /// <param name="serviceResponse"></param>
        private void AfterCommit(BaseModel baseModel, ServiceResponse serviceResponse)
        {
            //TODO: Xử lí sau khi lưu thành công và không còn transaction ( gửi mail, push noti)
            serviceResponse.Data = this.GetById(baseModel.GetType().Name, baseModel.GetPrimaryKeyValue().ToString());
        }

        #endregion

        #region Delete Data

        public ServiceResponse DeleteData(BaseModel baseModel)
        {
            ServiceResponse serviceResponse = new ServiceResponse();
            IDbTransaction transaction = null;
            IDbConnection connection = null;
            try
            {
                // Xử lí trước khi xoá
                BeforeDelete(baseModel);

                //Validate trước khi xoá
                var validateResults = ValidateSaveData(baseModel);
                if (validateResults.Count > 0)
                {
                    serviceResponse.ValidateInfo.AddRange(validateResults);
                    serviceResponse.Success = false;
                    return serviceResponse;
                }

                connection = this.GetDbConnection();
                connection.Open();
                transaction = connection.BeginTransaction();

                //xoá dữ liệu
                bool result = this.DoDelete(baseModel, transaction);
                //sau khi lưu thành công
                if (result)
                {
                    // Xử lí thêm nghiệp vụ sau khi xoá và còn transaction
                    AfterDelete(baseModel, serviceResponse,transaction);
                    //commit transaction
                    transaction.Commit();

                    //ghi log
                }
                else
                {
                    transaction.Rollback();
                    serviceResponse.Success = false;
                }
            }
            catch (Exception e)
            {
                if (transaction != null)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }

                throw e;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }

                if (connection != null && connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            if (serviceResponse.Success)
            {
                //Xử lí sau khi xoá, không còn trasaction
                AfterDeleteCommit(baseModel, serviceResponse);
            }

            return serviceResponse;
        }

        /// <summary>
        /// Thực hiển xử lí sau khi xoá xong và close transaction
        /// </summary>
        /// <param name="baseModel"></param>
        /// <param name="serviceResponse"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void AfterDeleteCommit(BaseModel baseModel, ServiceResponse serviceResponse)
        {
            //TODO: Thực hiển xử lí sau khi xoá xong và close transaction
        }

        /// <summary>
        /// Xử lí dữ liệu sau khi xoá, còn transaction
        /// </summary>
        /// <param name="baseModel"></param>
        /// <param name="transaction"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void AfterDelete(BaseModel baseModel,ServiceResponse serviceResponse, IDbTransaction transaction)
        {
            //TODO: Xử lí dữ liệu sau khi xoá, còn transaction
        }

        /// <summary>
        /// Chuẩn bị dữ liệu trước khi xoá
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool BeforeDelete(BaseModel baseModel)
        {
            //TODO: Chuẩn bị dữ liệu trước khi xoá
            return true;
        }

        public virtual bool DoDelete(BaseModel baseModel, IDbTransaction transaction)
        {
            //Kiểm tra có bảng liên detail không, nếu có bảng detail thì xoá trước khi xoá master
            var masterId = baseModel.GetPrimaryKeyValue();
            if (baseModel.ModelDetailConfigs != null && baseModel.ModelDetailConfigs.Count > 0)
            {
                foreach (var detailConfig in baseModel.ModelDetailConfigs.Where(c=> c.CascadeOnDeleteMasterModel))
                {
                    this.ExecuteDeleteByFieldNameAndValue(detailConfig.DetailTableName, detailConfig.ForeignKeyName,
                        masterId, transaction: transaction);
                }
            }
            // Xoá master
            this.ExecuteDeleteByFieldNameAndValue(baseModel.GetType().GetTableNameOnly(),
                baseModel.GetType().GetPrimaryKeyFieldName(), masterId, transaction);
            return true;
        }

        #endregion

        
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

        #region Notification.Ser

        public virtual void PushNotification(BaseModel baseModel, string action, string token,
            Dictionary<string, object> dicParam = null, DateTime? setTime = null)
        {
            //try
            //{
            //    //Các bước push notification
            //    /*
            //     * 1. Tạo tham số để build ra câu thôn gbaso: data dạng object {fieldName: value, fieldName2:value}
            //     * 2.Lấy câu thông báo
            //     * 3.Lấy danh sách user được nhận thông báo
            //     * 4.Lưu thông báo qua notification service
            //     * 5.Push notification cho client
            //     */

            //    //Lay user nhận thông báo
            //    var users = GetUserNotification(token, dicParam);
            //    var message = "";
            //    var notificationLink = "";
            //    var extraData =
            //        GetMobileNotificationExtraData(ref message, ref notificationLink, action, baseModel, dicParam);
            //    var rawData = GetRawDataNotification(ref message, ref notificationLink, action, baseModel, dicParam);
            //    if (users.Count == 0)
            //    {
            //        return;
            //    }
            //    List<PlatformNotification> platformNotifications=new List<PlatformNotification>();
            //    for (int i = 0; i < users.Count; i++)
            //    {
            //        PlatformNotification platformNotification=new PlatformNotification()
            //        {
            //            AppCode=ApplicationCode,
            //            Action=action,
            //            Message=message,
            //            SenderId=_userId,
            //            SenderName=_fullName,
            //            CreatedBy=_userName,
            //            CreatedDate=DateTime.Now,
            //            ModifiedBy=_userName,
            //            ModifiedDate=DateTime.Now,
            //            TenantId=_tenantId,
            //            UserId=_userId,
            //            ExtraData=extraData,
            //            RawData=rawData,
            //            NotificationLink=notificationLink,
            //            IsView=false,
            //            IsNew=true
            //        };
            //        platformNotifications.Add(platformNotification);
            //    }

            //    _notificationService.InsertNotification(platformNotifications, ApplicationCode, _tenantId, _userId,
            //        _fullName);
            //    CustomNotificationData(new List<PlatformNotification>(), action, baseModel, dicParam);
            //    NotificationContext notifyContext = new NotificationContext()
            //    {
            //        ApplicationCode=ApplicationCode,
            //        TenantId=_tenantId,
            //        UserId=_userId,
            //        AuthorizationToken=token,
            //        Action=action,
            //        Data=Converter.Serialize(platformNotifications),
            //        Event=CommonConstant.M_EVENT_DATA_CHANGED
            //    };
            //    if (setTime.HasValue)
            //    {
            //        ScheduleForPushNotification(platformNotifications, notifyContext,
            //            $"Notification_{baseModel.GetType().Name}_{baseModel.GetPrimaryKeyValue()}", setTime.Value);
            //    }
            //    else
            //    {
            //        //push thông báo cho client biết có thay đổi
            //        _pushNotificationService.PushNotification(notifyContext);
            //        foreach (var platformNotification in platformNotifications)
            //        {
            //            if (CheckPushNotificationMobile(platformNotification))
            //            {
            //                var mpnsKey = _configService.GetAppSetting("MPNS_APP_ID");
            //                if (!string.IsNullOrWhiteSpace(mpnsKey))
            //                {
            //                    // _pushNotificationService.PushNotification(platformNotification, mpnsKey,
            //                    //     _tenantId: _tenantId);
            //                }
            //            }
            //        }
            //    }

            //}
            //catch (Exception e)
            //{
            //    _logService.LogError(e, e.Message);
            //}
        }

        //public virtual bool CheckPushNotificationMobile(PlatformNotification notification)
        //{
        //    //lấy các action push cho mobile trong config
        //    var subsystemCode = _configService.GetAppSetting("SubSystemCodeNotify");
        //    var action = _configService.GetAppSetting("ActionPushNotifyMobile");
        //    var result = false;
        //    if (subsystemCode != null && action != null)
        //    {
        //        if (action.Contains(notification.Action))
        //        {
        //            result = true;
        //        }

        //    }

        //    return result;
        //}

        /// <summary>
        /// xử lí sau khi push thông báo, đặt lịch
        /// </summary>
        /// <param name="notifications"></param>
        /// <param name="notifyContext"></param>
        /// <param name="baseModelId"></param>
        /// <param name="setTime"></param>
        /// <exception cref="NotImplementedException"></exception>
        //public virtual void ScheduleForPushNotification(List<PlatformNotification> notifications, NotificationContext notifyContext, object baseModelId, DateTime setTime)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Chỉnh sửa lại notification sau khi tạo dữ liệu
        /// </summary>
        /// <param name="notifications"></param>
        /// <param name="action"></param>
        /// <param name="baseModel"></param>
        /// <param name="dicParam"></param>
        /// <exception cref="NotImplementedException"></exception>
        //public virtual void CustomNotificationData(List<PlatformNotification> notifications, string action, BaseModel baseModel, Dictionary<string, object> dicParam=null)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// Build data để merge vào thông báo
        /// </summary>
        /// <param name="message"></param>
        /// <param name="notificationLink"></param>
        /// <param name="action"></param>
        /// <param name="baseModel"></param>
        /// <param name="dicParam"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual Dictionary<string,string> GetRawDataNotification(ref string message, ref string notificationLink, string action, BaseModel baseModel, Dictionary<string, object> dicParam=null)
        {
            throw new Exception("DEV chua thi công lấy thông báo khi push notification, phải override lại");
        }

        /// <summary>
        /// Lấy thông báo notification
        /// </summary>
        /// <param name="message"></param>
        /// <param name="notificationLink"></param>
        /// <param name="action"></param>
        /// <param name="baseModel"></param>
        /// <param name="dicParam"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual Dictionary<string,string> GetMobileNotificationExtraData(ref string message, ref string notificationLink, string action, BaseModel baseModel, Dictionary<string, object> dicParam=null)
        {
            throw new Exception("DEV chua thi cong lấy thông báo khi push notification, phải override lại");
        }

        /// <summary>
        /// Lấy ra danh sách user được nhận thông báo
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="dicParam">danh sách tham số truyền vào thêm</param>
        /// <returns></returns>
        public  virtual List<Guid> GetUserNotification(string token, Dictionary<string, object> dicParam)
        {
            return new List<Guid>();
        }

        #endregion
        public List<ValidateResult> CheckPermissionUpdateField(FieldUpdate fieldUpdate)
        {
            throw new NotImplementedException();
        }
    }
}