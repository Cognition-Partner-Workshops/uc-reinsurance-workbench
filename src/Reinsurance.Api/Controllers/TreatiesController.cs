using System.Web.Http;
using Reinsurance.Services;
using Reinsurance.Services.DTOs;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/treaties")]
    public sealed class TreatiesController : ApiController
    {
        private readonly ITreatyService _service;
        public TreatiesController(ITreatyService service) { _service = service; }
        [HttpGet, Route("{id:int}")] public IHttpActionResult Get(int id) { var result = _service.Get(id); return result == null ? (IHttpActionResult)NotFound() : Ok(result); }
        [HttpPost, Route("")] public IHttpActionResult Post(TreatyCreateDto dto) { return Ok(_service.Create(dto)); }
        [HttpPost, Route("{id:int}/price")] public IHttpActionResult Price(int id) { var result = _service.Price(id); return result == null ? (IHttpActionResult)NotFound() : Ok(result); }
        [HttpPost, Route("{id:int}/bind")] public IHttpActionResult Bind(int id) { var result = _service.Bind(id); return result == null ? (IHttpActionResult)NotFound() : Ok(result); }
        [HttpGet, Route("{id:int}/pricing")] public IHttpActionResult Pricing(int id) { return Ok(_service.Pricing(id)); }
    }
}
