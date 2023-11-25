namespace Core.Contant
{
    public class AppSettingsKey
    {
        public const string ApplicationCode = "ApplicationCode";
        public const string LoginUrl = "LoginUrl";
        public const string DomainUrl = "DomainUrl";
        public const string InternalApiToken = "InternalApiToken";
        public const string ModelNamespace = "ModelNamespace";
        public const string AmisPlatformLoginPage = "AmisPlatformLoginPage";
        public const string MisaSupportEmail = "MisaSupportEmail";
        public const string MisaSupportPhone = "MisaSupportPhone";
        public const string RecruitmentWebsiteUrl = "RecruitmentWebsiteUrl";
        public const string EnumNameSpace = "EnumNameSpace";
        public const string ResourceNamespace = "ResourceNamespace";
        public const string QueryPath = "QueryPath";
        
        #region "MisaId"

        public const string ClientId = "ClientId";
        public const string ClientSecret = "ClientSecret";
        public const string MisaIdUrl = "MisaIdUrl";
        public const string JwtIssuer = "JwtIssuer";
        public const string JwtSecretKey = "JwtSecretKey";
        public const string MisaLoginRedirectUrl = "MisaLoginRedirectUrl";
        public const string MisaIdBaseUrl = "MisaIdBaseUrl";

        #endregion

        #region "Storage"

        public const string StorageServiceUrl = "StorageServiceUrl";
        public const string StorageAccessKey = "StorageAccessKey";
        public const string StorageSecretKey = "StorageSecretKey";
        public const string MaxFileSize = "MaxFileSize";
        public const string StorageBucketAmis3 = "StorageBucketAmis3";
        public const string FileUploadExtension = "FileUploadExtension";

        #endregion

        #region AppUrl
        public const string PlatformApiUrl = "PlatformApiUrl";
        public const string ManagementApiUrl = "ManagementApiUrl";
        

        #endregion

        #region Connect Facebook

        public const string FacebookGraphUrl = "FacebookGraphUrl";
        public const string FacebookAppId = "FacebookAppId";
        public const string FacebookClientSecret = "FacebookClientSecret";

        #endregion

        #region Payment Service
        public const string PaymentUrl = "PaymentUrl";
        public const string IsDemo = "IsDemo";
        

        #endregion

        #region ExtractCv Service
        public const string ExtractCvServiceUrl = "ExtractCVServiceUrl";
        

        #endregion

        #region Suman Service
        public const string SubscriberServiceUrl = "SubscriberServiceUrl";
        public const string LicenseServiceUrl = "LicenseServiceUrl";
        /// <summary>
        /// mã ứng dụng để check mã giảm giá trên suman
        /// </summary>
        public const string ApplicationId = "ApplicationId";
        /// <summary>
        /// email gửi khi thanh toán lỗ
        /// </summary>
        public const string EmailError = "EmailError";
        /// <summary>
        /// ngày hết hạn license
        /// </summary>
        public const string ExpireDate = "ExpireDate";
        /// <summary>
        /// Có phải là thanh toán demo hay không
        /// </summary>
        public const string IsLocal = "IsLocal";
        

        #endregion

        #region MisaOrder
        public const string MisaOrderUrl = "MisaOrderUrl";
        public const string MisaOrderServiceToken = "MisaOrderServiceToken";
        

        #endregion
    }
}