using Core.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Database
{
    public interface IDatabaseService
    {
        void RunCommandWithoutParameter(Action action);

        void RunCommandWithoutTenantIdCondition(Action action);

        #region Get

        Task<T> GetById<T>(Guid tenantId, string appCode, string id) where T : BaseModel;
        Task<dynamic> GetById(Guid tenantId, string appCode, string typeName, string id);

        Task<object> GetById(Guid tenantId, string appCode, Type modelType, string id);

        Task<List<T>> GetByListId<T>(Guid tenantId, string appCode, string ids) where T : BaseModel;

        Task<object> GetAll<T>(Guid tenantId, string appCode, string filter, string sort, string customFilter = "", WhereParameter whereParameter = null, string columns = "", string viewOrTableName = "") where T : BaseModel;

        Task<object> GetAll(Guid tenantId, string appCode, string typeName, string filter, string sort, string customFilter = "", WhereParameter whereParameter = null, string columns = "", string viewOrTableName = "");

        Task<PagingResponse> GetPagingUsingCommandText(Type typeModel, Guid tenantId, string appCode, int pageSize, int pageIndex,
            WhereParameter filter, List<GridSortItem> sorts, WhereParameter customFilter, WhereParameter fixedFilter = null, string columns = "",
            string viewOrTableName = "");
        Task<PagingResponse> GetPagingUsingCommandText(Type typeModel, Guid tenantId, string appCode, int pageSize, int pageIndex,
            string filter, string sort, string customFilter, WhereParameter fixedFilter = null, string columns = "",
            string viewOrTableName = "");

        /// <summary>
        /// Get Paging sửa dụng command text
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filter"></param>
        /// <param name="sorts"></param>
        /// <param name="customFilter"></param>
        /// <param name="column"></param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<PagingResponse> GetPagingUsingCommandText<T>(Guid tenantId, string appCode, int pageSize, int pageIndex,
            WhereParameter filter, List<GridSortItem> sorts, WhereParameter customFilter = null, WhereParameter fixedFilter = null, string columns = "",
            string viewOrTableName = "") where T : BaseModel;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="customFilter"></param>
        /// <param name="fixedFilter"></param>
        /// <param name="column"></param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<PagingResponse> GetPagingUsingCommandText<T>(Guid tenantId, string appCode, int pageSize, int pageIndex,
            string filter, string sort, string customFilter, WhereParameter fixedFilter = null, string columns = "",
            string viewOrTableName = "") where T : BaseModel;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeModel"></param>
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
        Task<PagingResponse> GetPagingUsingStoredProcedure<T>(Type typeModel, Guid tenantId, string appCode, int pageSize, int pageIndex,
            string filter, string sort, string customFilter, string storedProcedureName = "", Dictionary<string, object> param = null, WhereParameter fixedFilter = null, string column = "",
            string viewOrTableName = "") where T : BaseModel;

        Task<PagingResponse> GetPagingUsingStoredProcedure<T>(Guid tenantId, string appCode, int pageSize, int pageIndex,
            string filter, string sort, string customFilter, string storedProcedureName = "", Dictionary<string, object> param = null) where T : BaseModel;

        Task<PagingResponse> GetPagingUsingStoredProcedure(Type typeModel, Guid tenantId, string appCode, int pageSize, int pageIndex,
            string filter, string sort, string customFilter, string storedProcedureName = "", Dictionary<string, object> param = null, WhereParameter fixedFilter = null);
        /// <summary>
        /// lấy danh sách object detail theo master
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="modelType"></param>
        /// <param name="masterKey"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<dynamic> GetDetailByMaster(Guid tenantId, string appCode, string modelType, string masterKey,
            string filter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="modelType"></param>
        /// <param name="masterKey"></param>
        /// <param name="detailTypes"></param>
        /// <returns></returns>
        Task<Dictionary<string, List<object>>> GetListDetailByMaster(Guid tenantId, string appCode,
            string masterKey,
            List<string> detailTypes);
        /// <summary>
        /// Lấy danh sách detail theo master
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="masterType"></param>
        /// <param name="masterKey"></param>
        /// <returns></returns>
        Task<Dictionary<string, List<object>>> GetListDetailByMaster(Guid tenantId, string appCode, Type masterType,
            string masterKey);

        /// <summary>
        /// Lấy connection của từng app theo công ty
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <returns></returns>
        IDbConnection GetConnection(Guid tenantId, string appCode);


        #endregion

        #region ExecuteMethod
        /// <summary>
        /// Thực thi commantext trả về thành công hay thất bại
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<bool> ExecuteUsingCommandText(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam, IDbTransaction transaction = null, IDbConnection connection = null);
        /// <summary>
        /// Thực thi procedure trả về thành công hay thất bại
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<bool> ExecuteUsingStoredProcedure(Guid tenantId, string appCode, string storedProcedureName,
            object dicParam = null, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi một 1 commandText trả về 1 giá trị
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T">lại model</typeparam>
        /// <returns></returns>
        Task<T> ExecuteScalarUsingCommandText<T>(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam = null, IDbTransaction transaction = null, IDbConnection connection = null);
        /// <summary>
        /// Thực thi một storedProcedure trả về 1 giá trị
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> ExecuteScalarUsingStoredProcedure<T>(Guid tenantId, string appCode, string storedProcedureName,
            object dicParam = null, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// xoá 1 model
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="baseModel"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<bool> ExecuteDelete(Guid tenantId, string appCode, BaseModel baseModel, IDbTransaction transaction = null, IDbConnection connection = null);
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
        Task<bool> ExecuteDelete<T>(Guid tenantId, string appCode, T model, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Xoá theo key và value
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="modelType"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<bool> ExecuteDeleteByFieldNameAndValue(Guid tenantId, string appCode, Type modelType, string fieldName, object fieldValue, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Xoá bản ghi theo master ID
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<bool> ExecuteDeleteByFieldNameAndValue(Guid tenantId, string appCode, string tableName, string fieldName, object fieldValue, IDbTransaction transaction = null, IDbConnection connection = null);
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
        Task<bool> ExecuteUpdateField(Guid tenantId, string appCode, FieldUpdate fieldName, IDbTransaction transaction = null, IDbConnection connection = null);
        /// <summary>
        /// Check trùng dữ liệu bản ghi
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldKey"></param>
        /// <param name="valueKey"></param>
        /// <param name="fieldUnique"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<bool> ExecuteCheckDuplicateDate(Guid tenantId, string appCode, string tableName, string fieldKey, dynamic valueKey, Dictionary<string, object> fieldUnique, IDbTransaction transaction = null, IDbConnection connection = null);


        #endregion

        #region Query

        /// <summary>
        /// Thực thi một command text trả về danh sách
        /// </summary>
        /// <param name="tenantID"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> QueryUsingCommandText<T>(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi một storeProcedure trả về danh sách
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<List<T>> QueryUsingStoredProcedure<T>(Guid tenantId, string appCode, string storedProcedureName,
            object param = null, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi commandText và trả về danh sách dynamic
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> QueryUsingCommandText(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi command Text trả về danh sách dynamic
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="modelType"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<IEnumerable<object>> QueryUsingCommandText(Guid tenantId, string appCode, string commandText,
            Dictionary<string, object> dicParam, Type modelType, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi một storeProcedure trả về danh sách dynamic
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IEnumerable<dynamic>> QueryUsingStoredProcedure(Guid tenantId, string appCode, string storedProcedureName,
            object param = null, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi một command text trả về nhiều danh sách dữ liệu khác nhau
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="types"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<List<List<object>>> QueryMultipleUsingCommandText(Guid tenantId, string appCode, List<Type> types, string commandText,
            Dictionary<string, object> dicParam, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi một store trả về nhiều danh sách dữ liệu khác nhau
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="types"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <param name="commandText"></param>
        /// <returns></returns>
        Task<List<List<object>>> QueryMultipleUsingStoredProcedure(Guid tenantId, string appCode, List<Type> types, string storedProcedureName, object param = null, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi một command Text trả về nhiều danh sách dữ liệu
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="typeNames"></param>
        /// <param name="commandText"></param>
        /// <param name="dicParam"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<Dictionary<string, List<object>>> QueryMultipleUsingCommandText(Guid tenantId, string appCode,
            List<string> typeNames, string commandText, Dictionary<string, object> dicParam,
            IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi một store Procedure trả về nhiều danh sách dữ liệu 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="typeNames"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<Dictionary<string, List<object>>> QueryMultipleUsingStoredProcedure(Guid tenantId, string appCode, List<string> typeNames, string storedProcedureName, object param = null, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi một store procedure trả về nhiều danh sách dữ liệu
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="appCode"></param>
        /// <param name="storedProcedureName"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        Task<List<List<object>>> QueryMultipleUsingStoredProcedure(Guid tenantId, string appCode, string storedProcedureName, object param = null, IDbTransaction transaction = null, IDbConnection connection = null);


        #endregion
    }
}
