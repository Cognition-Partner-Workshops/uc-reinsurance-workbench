using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using NUnit.Framework;
using Reinsurance.Api;

namespace Reinsurance.Tests.Api
{
    [TestFixture]
    [Category("Unit")]
    public class ApiExceptionFilterTests
    {
        [Test]
        public void InternalErrorIncludesMatchingTraceHeaderAndBody()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/failure");
            request.SetConfiguration(new HttpConfiguration());
            var actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext { Request = request }
            };
            var executedContext = new HttpActionExecutedContext(
                actionContext,
                new InvalidOperationException("test failure"));

            new ApiExceptionFilter().OnException(executedContext);

            Assert.That(executedContext.Response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
            var body = executedContext.Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var bodyTraceId = Regex.Match(body, "\"traceId\":\"([^\"]+)\"").Groups[1].Value;
            var headerTraceId = executedContext.Response.Headers.GetValues("X-Trace-Id").Single();

            Assert.That(bodyTraceId, Is.Not.Empty);
            Assert.That(headerTraceId, Is.EqualTo(bodyTraceId));
        }
    }
}
