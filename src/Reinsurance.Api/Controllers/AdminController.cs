using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using Reinsurance.Data;
using Reinsurance.Data.Setup;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/admin")]
    public sealed class AdminController : ApiController
    {
        [HttpPost, Route("reseed")]
        public IHttpActionResult Reseed()
        {
            var expected = Environment.GetEnvironmentVariable("REINSURANCE_ADMIN_KEY") ?? "dev-admin";
            IEnumerable<string> values;
            if (!Request.Headers.TryGetValues("X-Admin-Key", out values) || values.FirstOrDefault() != expected)
                return Unauthorized();
            using (var context = new ReinsuranceObjectContext(WebApiApplication.ConnectionString()))
            {
                var tables = new[] { "PricingResults", "TreatyLayers", "Treaties", "ExposureRecords", "CatModelResults", "LossEvents", "Submissions", "ReferralRules", "PortfolioLimits", "Cedents", "Brokers", "Underwriters", "Perils", "Regions" };
                foreach (var table in tables)
                {
                    context.Database.ExecuteSqlCommand("DELETE FROM " + table);
                    context.Database.ExecuteSqlCommand("DBCC CHECKIDENT ('" + table + "', RESEED, 0) WITH NO_INFOMSGS");
                }
                DemoDataSeeder.Seed(context);
            }
            return Ok(new { status = "reseeded" });
        }
    }
}
