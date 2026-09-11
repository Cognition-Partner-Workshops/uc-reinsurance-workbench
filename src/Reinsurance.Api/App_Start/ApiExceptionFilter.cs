using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using Reinsurance.Core.Exceptions;
using Sentry;

namespace Reinsurance.Api
{
    public sealed class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;
            if (exception is OperationCanceledException) return;
            var traceId = Guid.NewGuid().ToString("N");
            var status = exception is EntityNotFoundException
                ? HttpStatusCode.NotFound
                : exception is InvalidSubmissionTransitionException || exception is PricingException
                    ? HttpStatusCode.Conflict
                    : HttpStatusCode.InternalServerError;
            if (status == HttpStatusCode.InternalServerError && SentrySdk.IsEnabled)
            {
                var routeTemplate = actionExecutedContext.Request.GetRouteData()?.Route?.RouteTemplate
                    ?? actionExecutedContext.Request.RequestUri?.AbsolutePath
                    ?? "unknown";
                SentrySdk.CaptureException(exception, scope =>
                {
                    scope.SetTag("traceId", traceId);
                    scope.SetTag("route", routeTemplate);
                });
            }
            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(status, new
            {
                title = status == HttpStatusCode.NotFound ? "Not found" : status == HttpStatusCode.Conflict ? "Request conflict" : "Unhandled exception",
                detail = exception.Message,
                traceId
            });
            actionExecutedContext.Response.Headers.Add("X-Trace-Id", traceId);
        }
    }
}
