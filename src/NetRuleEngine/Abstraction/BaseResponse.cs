namespace NetRuleEngine.Abstraction
{
    public class BaseResponse
    {
        public static readonly BaseResponse Ok = new BaseResponse { Code = 0 };
        public ErrorCode Code { get; set; }
        public string ErrorDescription { get; set; }
        public bool IsOk => this.Code == ErrorCode.OK;

        public static BaseResponse Error(ErrorCode code, string errorDescription)
        {
            return new BaseResponse { Code = code, ErrorDescription = errorDescription };
        }
    }

    public enum ErrorCode
    {
        OK = 0,
        InvalidParams = 400,
        TechnicalProblem = 401,
        InvalidDomain = 402,
        RestrictedIp,
        InvalidUrl,
        TryLater,
        NotMatch
    }
}
