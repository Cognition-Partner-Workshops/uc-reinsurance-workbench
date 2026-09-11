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

        public TreatiesController(ITreatyService treaties, IPricingService pricing)
        {
            _treaties = treaties;
            _pricing = pricing;
        }

        [HttpGet]
        [Route("{id:int}")]
        /// <summary>
        /// Gets a treaty by identifier.
        /// </summary>
        public IHttpActionResult Get(int id)
        {
            var result = _treaties.GetById(id);
            return result == null ? (IHttpActionResult)NotFound() : Ok(result);
        }

        [HttpGet]
        [Route("submission/{submissionId:int}")]
        /// <summary>
        /// Gets treaties for a submission.
        /// </summary>
        public IHttpActionResult BySubmission(int submissionId)
        {
            return Ok(_treaties.GetBySubmission(submissionId));
        }

        [HttpPost]
        [Route("")]
        /// <summary>
        /// Creates a treaty.
        /// </summary>
        public IHttpActionResult Post(TreatyCreateRequest request)
        {
            return Ok(_treaties.Create(request));
        }

        [HttpPost]
        [Route("{id:int}/layers")]
        /// <summary>
        /// Adds a layer to a treaty.
        /// </summary>
        public IHttpActionResult AddLayer(int id, TreatyLayerRequest request)
        {
            return Ok(_treaties.AddLayer(id, request));
        }

        [HttpPost]
        [Route("{id:int}/price")]
        /// <summary>
        /// Prices all layers on a treaty.
        /// </summary>
        public IHttpActionResult Price(int id)
        {
            return Ok(_pricing.PriceTreaty(id));
        }

        [HttpPost]
        [Route("{id:int}/bind")]
        /// <summary>
        /// Binds a treaty.
        /// </summary>
        public IHttpActionResult Bind(int id)
        {
            return Ok(_treaties.Bind(id));
        }

        [HttpGet]
        [Route("{id:int}/pricing")]
        /// <summary>
        /// Gets persisted pricing results for a treaty.
        /// </summary>
        public IHttpActionResult Pricing(int id)
        {
            return Ok(_pricing.GetPricing(id));
        }
    }
}
