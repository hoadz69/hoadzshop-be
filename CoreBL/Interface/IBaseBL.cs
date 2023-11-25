using System;
using System.Collections.Generic;
using System.Data;
using Core.Model;
using Core.Model.Platform;
using BaseModel = Core.Model.BaseModel;

namespace Core.BL
{
    public interface IBaseBL
    {
        string SubSystemCode { set; get; }

        /// <summary>
        /// lấy về misacode theo organizationUnitId
        /// </summary>
        /// <param name="organizationUnitId"></param>
        /// <returns></returns>
        string GetMisaCodeByOrganizationUnitId(Guid organizationUnitId);

        /// <summary>
        /// lấy danh sách quyền của người dùng
        /// </summary>
        /// <returns></returns>
        List<SC_ListRoleByApp> GetListRoles();

        void RunCommandWithoutParameter(Action action);
        
        
        void RunCommandWithoutTenantIdCondition(Action action);

        Dictionary<string, object> GetAllPermissionByApp();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="subSystemCode"></param>
        /// <param name="permissionCodes"></param>
        /// <param name="organizationUnitId"></param>
        /// <param name="isAndPermission"></param>
        /// <returns></returns>
        bool CheckPermission(string subSystemCode, string[] permissionCodes, Guid? organizationUnitId = null,
            bool isAndPermission = true);

        /// <summary>
        /// Hàm check quyền chức năng theo từng user
        /// </summary>
        /// <param name="baseModel"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        List<ValidateResult> CheckPermission(BaseModel baseModel, string[] action);

        /// <summary>
        /// Ghi đè action của quyền save 
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        string[] GetPermissionSaveData(BaseModel baseModel);

        /// <summary>
        /// Ghi đè action quyền xoá
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        string[] GetPermissionDeleteData(BaseModel baseModel);

        /// <summary>
        /// Hàm ghi đè action của quyền view
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        string[] GetPermissionView(BaseModel baseModel);

        /// <summary>
        /// Lấy chi tiết model theo id
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetById<T>(string id) where T : BaseModel;

        /// <summary>
        /// Kiểm tra xem user hiện tại phải admin không
        /// </summary>
        /// <returns></returns>
        bool IsAdmin();


        #region Get Method

        /// <summary>
        /// Lấy chi tiết model theo id, trả về dạng dynamic
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        dynamic GetById(string typeName, string id);

        /// <summary>
        /// Lấy chi tiết model theo id, trả về dạng dynamic
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        dynamic GetById(Type modelType, string id);

        /// <summary>
        /// Lấy danh sách thông tin master detail trả về client
        /// </summary>
        /// <param name="modelType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        dynamic GetFormData(Type modelType, string id);

        /// <summary>
        /// Lấy danh sách model theo danh sách id
        /// </summary>
        /// <param name="ids">chuỗi base64 của list id</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> GetByListId<T>(string ids) where T : BaseModel;

        /// <summary>
        /// Lấy tất cả dữ liệu của 1 bảng
        /// </summary>
        /// <param name="filter">chuỗi base64 của filter</param>
        /// <param name="sort">Chuỗi base64 của sort</param>
        /// <param name="customFilter">Chuỗi base64 của custom filter</param>
        /// <param name="whereParameter"></param>
        /// <param name="columns">các column cần lấy, ngăn cách bởi dấu ","</param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        object GetAll<T>(string filter, string sort, string customFilter = "", WhereParameter whereParameter = null,
            string columns = "", string viewOrTableName = "") where T : BaseModel;

        /// <summary>
        /// Lấy về tất cả dữ liệu của 1 bảng
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="customFilter"></param>
        /// <param name="whereParameter"></param>
        /// <param name="columns"></param>
        /// <param name="viewOrTableName"></param>
        /// <returns></returns>
        object GetAll(string typeName, string filter, string sort, string customFilter = "",
            WhereParameter whereParameter = null, string columns = "", string viewOrTableName = "");

        /// <summary>
        /// Get paging sử dụng commandText
        /// </summary>
        /// <param name="pageSize">số bản ghi/trang</param>
        /// <param name="pageIndex">Số thứ tự trang</param>
        /// <param name="filter">filter Where</param>
        /// <param name="sorts"></param>
        /// <param name="customFilter"></param>
        /// <param name="fixedFilter">điều kiện where fix sẵn</param>
        /// <param name="columns">các columns cần lấy, cách bởi dấu ","</param>
        /// <param name="viewOrTableName">tên view hoặc table( Không truyền mặc định lấy theo T)</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        PagingResponse GetPagingUsingCommandText<T>(int pageSize, int pageIndex, WhereParameter filter,
            List<GridSortItem> sorts, WhereParameter customFilter = null, WhereParameter fixedFilter = null,
            string columns = "", string viewOrTableName = "") where T : BaseModel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageSize">số bản ghi trên trang</param>
        /// <param name="pageIndex">số thứ tự trang</param>
        /// <param name="filter">chuỗi base64 của filter</param>
        /// <param name="sort">chỗi base64 của sort</param>
        /// <param name="customFilter">chỗi base64 của</param>
        /// <param name="fixedFilter">chỗi base64 của</param>
        /// <param name="columns">danh sách cột, ngăn cách bởi đấu ","</param>
        /// <param name="viewOrTableName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        PagingResponse GetPagingUsingCommanText<T>(int pageSize, int pageIndex, string filter, string sort,
            string customFilter, WhereParameter fixedFilter = null, string columns = "", string viewOrTableName = "")
            where T : BaseModel;

        /// <param name="typeModel">loại model</param>
        /// <param name="pageSize">số bản ghi trên trang</param>
        /// <param name="pageIndex">số thứ tự trang</param>
        /// <param name="filter">chuỗi base64 của filter</param>
        /// <param name="sort">chỗi base64 của sort</param>
        /// <param name="customFilter">chỗi base64 của</param>
        /// <param name="fixedFilter">chỗi base64 của</param>
        /// <param name="columns">danh sách cột, ngăn cách bởi đấu ","</param>
        /// <param name="viewOrTableName"></param>
        PagingResponse GetPagingUsingCommanText(Type typeModel, int pageSize, int pageIndex, string filter, string sort,
            string customFilter, WhereParameter fixedFilter = null, string columns = "", string viewOrTableName = "");

        /// <summary>
        /// Get paging sử dụng StoredProcedure
        /// </summary>
        /// <param name="pageSize">số bản ghi trên trang</param>
        /// <param name="pageIndex">số thứ tự trang</param>
        /// <param name="filter">chuỗi base64 của filter</param>
        /// <param name="sort">chuỗi base64 của sort</param>
        /// <param name="customFilter">chỗi base64 của</param>
        /// <param name="storedProcedure">tên store, không truyền sẽ tự gọi Proc_{modelType}_GetPaging</param>
        /// <param name="param">danh sách tham số dạng dictionnary</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        PagingResponse GetPagingUsingStoredProcedure<T>(int pageSize, int pageIndex, string filter, string sort,
            string customFilter, string storedProcedure = "", Dictionary<string, object> param = null)
            where T : BaseModel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeModel">loại model</param>
        /// <param name="pageSize">số bản ghi trên trang</param>
        /// <param name="pageIndex">số thứ tự trang</param>
        /// <param name="filter">chuỗi base64 của filter</param>
        /// <param name="sort">chuỗi base64 của sort</param>
        /// <param name="customFilter">chỗi base64 của</param>
        /// <param name="storedProcedure">tên store, không truyền sẽ tự gọi Proc_{modelType}_GetPaging</param>
        /// <param name="param">danh sách tham số</param>
        /// <param name="fixedFilter"></param>
        /// <returns></returns>
        PagingResponse GetPagingUsingStoredProcedure(Type typeModel, int pageSize, int pageIndex, string filter,
            string sort,
            string customFilter, string storedProcedure = "", Dictionary<string, object> param = null,
            WhereParameter fixedFilter = null);

        /// <summary>
        /// Lấy danh sách object detail theo master ids
        /// </summary>
        /// <param name="modelType">loại model</param>
        /// <param name="masterKey">Giá trị khoá chính của master model</param>
        /// <param name="filter">chuỗi base64 của filter</param>
        /// <returns></returns>
        dynamic GetDetailByMaster(string modelType, string masterKey, string filter);

        /// <summary>
        /// lấy danh sách các loại detail theo id master
        /// </summary>
        /// <param name="masterKey">giá trị khoá chính của master model</param>
        /// <param name="detailType">các loại detail model</param>
        /// <returns></returns>
        Dictionary<string, List<object>> GetListDetailByMaster(string masterKey, List<string> detailTypes);
        
        /// <summary>
        /// lấy danh sách các detail của type master
        /// </summary>
        /// <param name="masterKey"></param>
        /// <param name="masterType"></param>
        /// <returns></returns>
        Dictionary<string, List<object>> GetListDetailByMaster(string masterKey, Type masterType);

        /// <summary>
        /// lấy connection theo từng app từng công ty
        /// </summary>
        /// <returns></returns>
        IDbConnection GetDbConnection();

        #endregion

        #region Execute Method

        /// <summary>
        /// Thực thi command text trả về 1 giá trị
        /// </summary>
        /// <param name="commandText">command Text</param>
        /// <param name="dicParam">từ điển tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        bool ExecuteUsingCommandText(string commandText, Dictionary<string, object> dicParam,
            IDbTransaction transaction = null, IDbConnection connection = null);
        
        /// <summary>
        ///  Thực thi một store trả về 1 giá trị
        /// </summary>
        /// <param name="storedProcedureName">Tên storedProcedure</param>
        /// <param name="param">object/từ điển tham số của storedProcedure</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        bool ExecuteUsingStoredProcedure(string storedProcedureName,  object param,
            IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Thực thi command text trả về 1 giá trị
        /// </summary>
        /// <param name="commandText">command Text</param>
        /// <param name="dicParam">từ điển tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T">loại model</typeparam>
        /// <returns></returns>
        T ExecuteScalarUsingCommandText<T>(string commandText, Dictionary<string, object> dicParam = null,
            IDbTransaction transaction = null, IDbConnection connection = null);


        /// <summary>
        ///  Thực thi một store trả về 1 giá trị
        /// </summary>
        /// <param name="storedProcedureName">Tên storedProcedure</param>
        /// <param name="param">object/từ điển tham số của storedProcedure</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T ExecuteScalarUsingStoredProcedure<T>(string storedProcedureName,  object param,
            IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Xoá 1 model
        /// </summary>
        /// <param name="model">object model</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T">loại model</typeparam>
        /// <returns></returns>
        bool ExecuteDelete<T>(T model, IDbTransaction transaction = null, IDbConnection connection = null)
            where T : BaseModel;

        /// <summary>
        /// Xoá 1 model
        /// </summary>
        /// <param name="model">object model</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        bool ExecuteDelete(BaseModel model, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Xoá bản ghi theo theo một trường và giá trị
        /// </summary>
        /// <param name="tableName">Tên bảng trong database</param>
        /// <param name="fieldName">tên field làm điều kiện xoá</param>
        /// <param name="fieldValue">giá trị của field điều kiện</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        bool ExecuteDeleteByFieldNameAndValue(string tableName, string fieldName, object fieldValue, IDbTransaction transaction = null, IDbConnection connection = null);
        
        /// <summary>
        /// Xoá bản ghi theo theo một trường và giá trị
        /// </summary>
        /// <param name="modelType">loại model</param>
        /// <param name="fieldName">tên field làm điều kiện xoá</param>
        /// <param name="fieldValue">giá trị của field điều kiện</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        bool ExecuteDeleteByFieldNameAndValue(Type modelType, string fieldName, object fieldValue, IDbTransaction transaction = null, IDbConnection connection = null);

        /// <summary>
        /// Update thông tin 1 trường theo khoá
        /// </summary>
        /// <param name="fieldUpdate">Data field update</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        ServiceResponse ExecuteUpdateField(FieldUpdate fieldUpdate, IDbTransaction transaction = null,
            IDbConnection connection = null);

        #endregion

        #region Query Method

        /// <summary>
        /// Thực thi một commandText trả về danh sách
        /// </summary>
        /// <param name="commandText">commandText cần thực thi</param>
        /// <param name="dicParam">từ điển các tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T">loại model</typeparam>
        /// <returns></returns>
        List<T> QueryUsingCommandText<T>(string commandText, Dictionary<string, object> dicParam,
            IDbTransaction transaction = null,
            IDbConnection connection = null);

        /// <summary>
        /// Thực thi một commandText trả về danh sách
        /// </summary>
        /// <param name="commandText">commandText cần thực thi</param>
        /// <param name="dicParam">từ điển các tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        IEnumerable<dynamic> QueryUsingCommandText(string commandText, Dictionary<string, object> dicParam,
            IDbTransaction transaction = null,
            IDbConnection connection = null);
        /// <summary>
        /// Thực thi một storedProcedure trả về danh sách
        /// </summary>
        /// <param name="storedProcedureName">tên storedProcedure</param>
        /// <param name="param">object/từ điển tham số của StoredProcedure</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> QueryUsingStoredProcedure<T>(string storedProcedureName,  object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null);
        /// <summary>
        /// Thực thi một storedProcedure trả về danh sách
        /// </summary>
        /// <param name="storedProcedureName">tên storedProcedure</param>
        /// <param name="param">object/từ điển tham số của StoredProcedure</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        IEnumerable<dynamic> QueryUsingStoredProcedure(string storedProcedureName,  object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null);

        /// <summary>
        /// Thực thi một command trả về nhiều danh sách dữ liệu
        /// </summary>
        /// <param name="types">các loại dữ liệu trả về</param>
        /// <param name="commandText">commandText cần thực thi</param>
        /// <param name="dicParam">từ điển các tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        List<List<object>> QueryMultipleUsingCommandText(List<Type> types, string commandText,
            Dictionary<string, object> dicParam,
            IDbTransaction transaction = null,
            IDbConnection connection = null);
        /// <summary>
        /// Thực thi 1 storedProcedure trả về nhiều danh sách dữ liệu
        /// </summary>
        /// <param name="types">các loại dữ liệu trả về</param>
        /// <param name="storedProcedureName">tên storedProcedure</param>
        /// <param name="param">object/từ điển tham số của StoredProcedure</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        List<List<object>> QueryMultipleUsingStoredProcedure(List<Type>types,string storedProcedureName,  object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null);
        /// <summary>
        /// Thực thi một command trả về nhiều danh sách dữ liệu
        /// </summary>
        /// <param name="typeNames">tên các loại dữ liệu trả về</param>
        /// <param name="commandText">commandText cần thực thi</param>
        /// <param name="dicParam">từ điển các tham số của commandText</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        Dictionary<string,List<object>> QueryMultipleUsingCommandText(List<string> typeNames, string commandText,
            Dictionary<string, object> dicParam,
            IDbTransaction transaction = null,
            IDbConnection connection = null);
        
        /// <summary>
        /// Thực thi 1 storedProcedure trả về nhiều danh sách dữ liệu
        /// </summary>
        /// <param name="typeNames">tên các loại dữ liệu trả về</param>
        /// <param name="storedProcedureName">tên storedProcedure</param>
        /// <param name="param">object/từ điển tham số của StoredProcedure</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        Dictionary<string,List<object>> QueryMultipleUsingStoredProcedure(List<string>typeNames,string storedProcedureName,  object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null);
        /// <summary>
        /// Thực thi 1 storedProcedure trả về nhiều danh sách dữ liệu
        /// </summary>
        /// <param name="storedProcedureName">tên storedProcedure</param>
        /// <param name="param">object/từ điển tham số của StoredProcedure</param>
        /// <param name="transaction">Transaction thực thi</param>
        /// <param name="connection">Connection thực thi</param>
        /// <returns></returns>
        List<List<object>> QueryMultipleUsingStoredProcedure(string storedProcedureName,  object param,
            IDbTransaction transaction = null,
            IDbConnection connection = null);
        #endregion

        /// <summary>
        /// Cất dữ liệu vào database
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        ServiceResponse SaveData(BaseModel baseModel);

        /// <summary>
        /// Xoá dữ liệu
        /// </summary>
        /// <param name="baseModel"></param>
        /// <returns></returns>
        ServiceResponse DeleteData(BaseModel baseModel);

        /// <summary>
        /// Check quyền update field
        /// </summary>
        /// <param name="fieldUpdate"></param>
        /// <returns></returns>
        List<ValidateResult> CheckPermissionUpdateField(FieldUpdate fieldUpdate);

        /// <summary>
        /// Get Paging theo storedProcedure hoặc commandText
        /// </summary>
        /// <param name="typeModel">loại model</param>
        /// <param name="pagingRequest">config paging</param>
        /// <param name="storedProcedureName">tên stored</param>
        /// <param name="param">danh sách từ điển tham số</param>
        /// <returns></returns>
        PagingResponse GetPaging(Type typeModel, PagingRequest pagingRequest, string storedProcedureName = "",
            Dictionary<string, object> param = null);
    }
}