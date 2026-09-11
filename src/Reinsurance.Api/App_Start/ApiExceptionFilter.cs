using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Sentry;

namespace Reinsurance.Api
{
    public sealed class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;
            if (SentrySdk.IsEnabled) SentrySdk.CaptureException(exception);
            var traceId = Guid.NewGuid().ToString("N");
            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError, new
            {
                title = "Unhandled exception",
                detail = exception.Message,
                traceId
            });
        }
    }
}
