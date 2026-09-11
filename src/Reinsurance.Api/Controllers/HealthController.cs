using System;
using System.Web.Http;
using Reinsurance.Data;
using Sentry;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/health")]
    public sealed class HealthController : ApiController
    {
        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            var db = "ok";
            try
            {
                using (var context = new ReinsuranceObjectContext(WebApiApplication.ConnectionString()))
                    context.Database.Connection.Open();
            }
            catch (Exception)
            {
                db = "down";
            }
            return Ok(new { status = db == "ok" ? "ok" : "degraded", db, sentry = SentrySdk.IsEnabled });
        }
    }
}
