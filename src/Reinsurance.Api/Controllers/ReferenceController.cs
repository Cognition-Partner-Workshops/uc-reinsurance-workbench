using System.Web.Http;
using Reinsurance.Services;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/reference")]
    public sealed class ReferenceController : ApiController
    {
        private readonly IReferenceService _service;
        public ReferenceController(IReferenceService service) { _service = service; }
        [HttpGet, Route("perils")] public IHttpActionResult Perils() { return Ok(_service.Perils()); }
        [HttpGet, Route("regions")] public IHttpActionResult Regions() { return Ok(_service.Regions()); }
        [HttpGet, Route("referral-rules")] public IHttpActionResult ReferralRules() { return Ok(_service.ReferralRules()); }
    }
}
