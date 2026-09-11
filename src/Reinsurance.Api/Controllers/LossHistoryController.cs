using System.Web.Http;
using Reinsurance.Services.LossHistory;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/loss-history")]
    public sealed class LossHistoryController : ApiController
    {
        private readonly ILossHistoryService _service;
        public LossHistoryController(ILossHistoryService service) { _service = service; }
        [HttpGet, Route("")] public IHttpActionResult Get(int cedentId) { return Ok(new { events = _service.GetByCedent(cedentId), burningCost = _service.BurningCost(cedentId, 5) }); }
    }
}
