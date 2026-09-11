using System.Web.Http;
using Reinsurance.Services;
using Reinsurance.Services.DTOs;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/submissions")]
    public sealed class SubmissionsController : ApiController
    {
        private readonly ISubmissionService _service;
        public SubmissionsController(ISubmissionService service) { _service = service; }
        [HttpGet, Route("")] public IHttpActionResult Get() { return Ok(_service.GetAll()); }
        [HttpGet, Route("{id:int}")] public IHttpActionResult Get(int id) { var result = _service.Get(id); return result == null ? (IHttpActionResult)NotFound() : Ok(result); }
        [HttpPost, Route("")] public IHttpActionResult Post(SubmissionCreateDto dto) { return Ok(_service.Create(dto)); }
        [HttpPost, Route("{id:int}/transition")] public IHttpActionResult Transition(int id, TransitionDto dto) { var result = _service.Transition(id, dto); return result == null ? (IHttpActionResult)NotFound() : Ok(result); }
        [HttpGet, Route("{id:int}/exposure")] public IHttpActionResult Exposure(int id) { return Ok(_service.Exposure(id)); }
        [HttpGet, Route("{id:int}/cat-model")] public IHttpActionResult CatModel(int id) { return Ok(_service.CatModel(id)); }
    }
}
