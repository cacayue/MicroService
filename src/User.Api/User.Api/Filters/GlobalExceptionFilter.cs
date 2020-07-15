using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace User.Api.Filters
{
    public class GlobalExceptionFilter:IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger,IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            var json = new JsonErrorResponse();
            if (_env.IsDevelopment())
            {
                json.DevelopMessage = context.Exception.StackTrace;
            }
            if (context.Exception.GetType() == typeof(UserOperationException))
            {
                json.Message = context.Exception.Message;
               
                context.Result = new BadRequestObjectResult(json);
            }
            else
            {
                json.Message = "发生了未知的内部错误";
                context.Result = new InternalServerErrorObjectResult(json);
            }

            _logger.LogError(context.Exception,context.Exception.Message);
            context.ExceptionHandled = true;
        }
    }

    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object error):base(error)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}