using System.Web.Http;
using Reinsurance.Services.Reference;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/reference")]
    public sealed class ReferenceController : ApiController
    {
        private readonly IReferenceService _service;
        public ReferenceController(IReferenceService service) { _service = service; }
        [HttpGet, Route("perils")] public IHttpActionResult Perils() { return Ok(_service.GetPerils()); }
        [HttpGet, Route("regions")] public IHttpActionResult Regions() { return Ok(_service.GetRegions()); }
        [HttpGet, Route("referral-rules")] public IHttpActionResult ReferralRules() { return Ok(_service.GetReferralRules()); }
    }
}
