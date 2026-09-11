using System.Web.Http;
using Sentry;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/health")]
    public sealed class HealthController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            return Ok(new { status = "ok", sentry = SentrySdk.IsEnabled });
        }
    }
}
