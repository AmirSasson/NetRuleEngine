

namespace NetRuleEngine.Abstraction
{
    public class BaseDataResponse<T> : BaseResponse
    {       
        public T Data { get; set; }
    }        
}
