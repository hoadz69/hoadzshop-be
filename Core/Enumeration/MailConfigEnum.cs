namespace Core.Enumeration
{
    /// <summary>
    /// Dữ liệu trả về khi kiểm tra cấu hình mail
    /// </summary>
    public enum MailConfigResponse
    {
        Success=200,
        NotFound=404,
        Failed=501,
        UnknowHost=1,
        WrongPort=2,
        WropSecurityMethod=3,
        NotAuthen=4,
        ValidateError=5,
    }

    /// <summary>
    /// Mail có yêu cầu dịch vụ hay không
    /// </summary>
    public enum IsServiceRequired
    {
        No=0,
        Yes=1
    }

    /// <summary>
    /// Có gửi mail hay không
    /// </summary>
    public enum IsSendMail
    {
        No=0,
        Yes=1
    }
    /// <summary>
    /// Kiểu phương thức bảo mật của mail
    /// </summary>
    public enum MailSecurityMethodEnum : int
    {
        None=0,
        SSL=1,
        TLS=2
    }
}