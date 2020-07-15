using Microsoft.AspNetCore.Mvc;

namespace User.Api
{
    public class JsonErrorResponse
    {
        public string Message { get; set; }

        public object DevelopMessage { get; set; }
    }
}