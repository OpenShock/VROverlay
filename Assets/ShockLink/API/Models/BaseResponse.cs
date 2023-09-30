// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ShockLink.API.Models
{
    public class BaseResponse<T>
    {
        public string? Message { get; set; }
        public T? Data { get; set; }

        public BaseResponse(string? message = null, T? data = default)
        {
            Message = message;
            Data = data;
        }
    }
}