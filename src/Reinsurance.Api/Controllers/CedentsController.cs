using System.Web.Http;
using Reinsurance.Services.Cedents;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/cedents")]
    public sealed class CedentsController : ApiController
    {
        private readonly ICedentService _service;

        public CedentsController(ICedentService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("")]
        /// <summary>
        /// Gets all cedents.
        /// </summary>
        public IHttpActionResult Get()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet]
        [Route("{id:int}")]
        /// <summary>
        /// Gets a cedent by identifier.
        /// </summary>
        public IHttpActionResult Get(int id)
        {
            var result = _service.GetById(id);
            return result == null ? (IHttpActionResult)NotFound() : Ok(result);
        }
    }
}
