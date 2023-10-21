
namespace MonitorQA.Api.Infrastructure.Models
{
    public class InternalResultModel
    {
        public bool IsSuccessStatusCode => string.IsNullOrEmpty(Error);

        public string Error { get; set; }

        public object GetApiErrorResponseObject()
        {
            return new {ErrorCode = Error};
        }
    }
}
