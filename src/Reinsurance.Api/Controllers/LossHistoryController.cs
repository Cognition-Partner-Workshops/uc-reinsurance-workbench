using System.Web.Http;
using Reinsurance.Services;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/loss-history")]
    public sealed class LossHistoryController : ApiController
    {
        private readonly ILossHistoryService _service;
        public LossHistoryController(ILossHistoryService service) { _service = service; }
        [HttpGet, Route("")] public IHttpActionResult Get(int cedentId) { return Ok(_service.Get(cedentId)); }
    }
}
