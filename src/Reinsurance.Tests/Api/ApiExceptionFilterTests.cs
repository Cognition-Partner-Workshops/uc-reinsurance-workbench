using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using NUnit.Framework;
using Reinsurance.Api;
using Reinsurance.Tests.Fakes;
using Sentry;

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

        [Test]
        public void ClientCancellationIsNotConvertedToInternalError()
        {
            var executedContext = ExecutedContext(new TaskCanceledException());

            new ApiExceptionFilter().OnException(executedContext);

            Assert.That(executedContext.Response, Is.Null);
        }

        [Test]
        public void ClientCancellationIsNotCapturedBySentry()
        {
            var transport = new FakeTransport();
            SentrySdk.Init(options =>
            {
                options.Dsn = "https://public@sentry.invalid/1";
                options.Transport = transport;
                options.FlushTimeout = TimeSpan.FromSeconds(5);
            });
            try
            {
                Assert.That(SentrySdk.IsEnabled, Is.True);

                new ApiExceptionFilter().OnException(ExecutedContext(new TaskCanceledException()));
                SentrySdk.Flush(TimeSpan.FromSeconds(5));
                Assert.That(transport.Envelopes, Is.Empty);

                new ApiExceptionFilter().OnException(ExecutedContext(new InvalidOperationException("test failure")));
                SentrySdk.Flush(TimeSpan.FromSeconds(5));
                Assert.That(transport.Envelopes, Has.Count.EqualTo(1));
            }
            finally
            {
                SentrySdk.Close();
            }
        }

        private static HttpActionExecutedContext ExecutedContext(Exception exception)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/health");
            request.SetConfiguration(new HttpConfiguration());
            var actionContext = new HttpActionContext
            {
                ControllerContext = new HttpControllerContext { Request = request }
            };
            return new HttpActionExecutedContext(actionContext, exception);
        }
    }
}
