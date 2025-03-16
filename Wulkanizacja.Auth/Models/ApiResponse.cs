namespace Wulkanizacja.Auth.Models
{
    public class ApiResponse
    {
        public string Code { get; set; }
        public string Reason { get; set; }
        public ApiResponse(string code, string reason)
        {
            Code = code;
            Reason = reason;
        }
    }
}
