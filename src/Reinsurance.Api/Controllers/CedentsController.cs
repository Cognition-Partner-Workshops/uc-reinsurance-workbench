using System.Web.Http;
using Reinsurance.Services;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/cedents")]
    public sealed class CedentsController : ApiController
    {
        private readonly ICedentService _service;
        public CedentsController(ICedentService service) { _service = service; }
        [HttpGet, Route("")] public IHttpActionResult Get() { return Ok(_service.GetAll()); }
        [HttpGet, Route("{id:int}")] public IHttpActionResult Get(int id) { var result = _service.Get(id); return result == null ? (IHttpActionResult)NotFound() : Ok(result); }
    }
}
