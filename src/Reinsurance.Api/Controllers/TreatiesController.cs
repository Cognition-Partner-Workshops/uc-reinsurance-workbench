using System.Web.Http;
using Reinsurance.Services.Pricing;
using Reinsurance.Services.Treaties;
using Reinsurance.Services.Treaties.Models;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/treaties")]
    public sealed class TreatiesController : ApiController
    {
        private readonly ITreatyService _treaties;
        private readonly IPricingService _pricing;
        public TreatiesController(ITreatyService treaties, IPricingService pricing) { _treaties = treaties; _pricing = pricing; }
        [HttpGet, Route("{id:int}")] public IHttpActionResult Get(int id)
        {
            var result = _treaties.GetById(id);
            return result == null ? (IHttpActionResult)NotFound() : Ok(result);
        }
        [HttpGet, Route("submission/{submissionId:int}")] public IHttpActionResult BySubmission(int submissionId) { return Ok(_treaties.GetBySubmission(submissionId)); }
        [HttpPost, Route("")] public IHttpActionResult Post(TreatyCreateRequest request) { return Ok(_treaties.Create(request)); }
        [HttpPost, Route("{id:int}/layers")] public IHttpActionResult AddLayer(int id, TreatyLayerRequest request) { return Ok(_treaties.AddLayer(id, request)); }
        [HttpPost, Route("{id:int}/price")] public IHttpActionResult Price(int id) { return Ok(_pricing.PriceTreaty(id)); }
        [HttpPost, Route("{id:int}/bind")] public IHttpActionResult Bind(int id) { return Ok(_treaties.Bind(id)); }
        [HttpGet, Route("{id:int}/pricing")] public IHttpActionResult Pricing(int id) { return Ok(_pricing.GetPricing(id)); }
    }
}
