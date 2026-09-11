using System.Web.Http;
using Reinsurance.Services.Portfolio;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/portfolio")]
    public sealed class PortfolioController : ApiController
    {
        private readonly IPortfolioService _service;

        public PortfolioController(IPortfolioService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("")]
        /// <summary>
        /// Gets every submission with its treaties, pricing, cat-model results and exposure summary in one response.
        /// </summary>
        public IHttpActionResult Get()
        {
            return Ok(_service.Get());
        }
    }
}
