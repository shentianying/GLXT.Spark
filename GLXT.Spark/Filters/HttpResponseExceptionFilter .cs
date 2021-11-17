using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using GLXT.Spark.Entity.XTGL;
using GLXT.Spark.IService;

namespace GLXT.Spark.Filters
{
    public class HttpResponseExceptionFilter : IActionFilter
    {
        private readonly ISystemService _systemService;

        public HttpResponseExceptionFilter(ISystemService systemService)
        {
            _systemService = systemService;
        }
        public void OnActionExecuting(ActionExecutingContext context) { }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is Exception exception)
            {
                // --------------捕获错误后的操作--------------------
                context.Result = new ObjectResult(exception.ToString())
                {
                    StatusCode = StatusCodes.Status200OK,
                };
                string uname = context.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
                int uid = int.Parse(context.HttpContext.User.FindFirst(ClaimTypes.Sid).Value);

                var se = new SystemExceptions()
                {
                    ErrorInfo = exception.ToString(),
                    CreateTime = DateTime.Now,
                    TriggerUserName = uname,
                    TriggerUserId = uid
                };
                _systemService.AddExceptions(se);
                context.ExceptionHandled = true;
            }
        }
    }
}
