using System;
using System.Collections.Generic;
using Core.Enumeration;

namespace Core.Model
{
    public class ServiceResponse
    {
        private List<ValidateResult> _validateInfo;
        public const string DEFAULT_ERRORMESSAGE = "Có lỗi trong quá trình xử lí";

        /// <summary>
        /// Kết quả kiểm tra dữ liệu khi thực hiện cất xoá
        /// </summary>
        public List<ValidateResult> ValidateInfo
        {
            get
            {
                if (this._validateInfo == null)
                {
                    this._validateInfo=new List<ValidateResult>();
                }

                return this._validateInfo;
            }
            set { this._validateInfo = value; }
        }
        /// <summary>
        /// Kết quả thực hiện
        /// </summary>
        public bool Success { set; get; } = true;
         /// <summary>
         /// Mã lỗi chính
         /// </summary>
        public ServiceResponseCode Code { set; get; } = ServiceResponseCode.Succes;
        /// <summary>
        /// Mã lỗi phụ, phân biệt chi tiết các trường hợp ở lỗi chính
        /// </summary>
        public int SubCode { get; set; }
        /// <summary>
        /// Nội dung lỗi hiển thị cho người dùng
        /// </summary>
        public string UserMessage { get; set; }
        /// <summary>
        /// Nội dung lỗi của hệ thống ( phục vụ điều tra lỗi)
        /// </summary>
        public string SystemMessage { get; set; }
        /// <summary>
        /// Dữ liệu trả về
        /// </summary>
        public object Data { get; set; }
        
        public DateTime ServerTime { set; get; }

        public ServiceResponse()
        {
            
        }

        public override string ToString()
        {
            if (Success)
            {
                return $"Success";
            }
            else
            {
                return $"Failed - Code: {Code}-{SubCode} - SysMessage: {SystemMessage} - UserMessage: {UserMessage}";
            }
        }
        /// <summary>
        /// Dữ liệu trả về khi success
        /// </summary>
        /// <returns></returns>
        public ServiceResponse OnSuccess()
        {
            this.Data = Data;
            return this;
        }
        /// <summary>
        /// Dữ liệu trả về khi gặp Exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public ServiceResponse OnException(Exception ex)
        {
            if (ex != null)
            {
                this.Success = false;
                this.Code = ServiceResponseCode.Error;
                this.UserMessage = DEFAULT_ERRORMESSAGE;
#if true
                this.SystemMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    this.SystemMessage = Environment.NewLine + ex.InnerException.Message;
                }
#else
                this.SystemMessage="Exception"
#endif
            }

            return this;
        }
        /// <summary>
        /// Dữ liệu trả về khi gặp lỗi
        /// </summary>
        /// <param name="subCode"></param>
        /// <param name="userMessage"></param>
        /// <param name="systemMessage"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ServiceResponse OnError(int subCode=0, string userMessage=DEFAULT_ERRORMESSAGE, string systemMessage="", object data=null )
        {
            this.Success = false;
            this.Code = ServiceResponseCode.Error;
            this.SubCode = subCode;
            this.UserMessage = userMessage;
            this.SystemMessage = systemMessage;
            this.Data = data;
            if (string.IsNullOrEmpty(systemMessage))
            {
                this.SystemMessage = $"{(int) (ServiceResponseCode.Error)}-{SubCode}";
            }

            return this;
        }
    }
}