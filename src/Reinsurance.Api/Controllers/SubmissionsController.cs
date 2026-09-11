using System.Web.Http;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Services.CatModel;
using Reinsurance.Services.Submissions;
using Reinsurance.Services.Submissions.Models;

namespace Reinsurance.Api.Controllers
{
    [RoutePrefix("api/submissions")]
    public sealed class SubmissionsController : ApiController
    {
        private readonly ISubmissionService _service;
        private readonly ICatModelResultService _catModels;

        public SubmissionsController(ISubmissionService service, ICatModelResultService catModels)
        {
            _service = service;
            _catModels = catModels;
        }

        [HttpGet]
        [Route("")]
        /// <summary>
        /// Searches submissions.
        /// </summary>
        public IHttpActionResult Get(SubmissionStatus? status = null, int? cedentId = null, int page = 1, int pageSize = 20)
        {
            var result = _service.Search(new SubmissionSearchQuery { Status = status, CedentId = cedentId, PageIndex = page <= 0 ? 0 : page - 1, PageSize = pageSize });
            return Ok(new { items = result, page = result.PageNumber, pageSize = result.PageSize, totalCount = result.TotalCount, totalPages = result.TotalPages });
        }

        [HttpGet]
        [Route("{id:int}")]
        /// <summary>
        /// Gets a submission by identifier.
        /// </summary>
        public IHttpActionResult Get(int id)
        {
            var result = _service.GetById(id);
            return result == null ? (IHttpActionResult)NotFound() : Ok(result);
        }

        [HttpPost]
        [Route("")]
        /// <summary>
        /// Creates a submission.
        /// </summary>
        public IHttpActionResult Post(SubmissionCreateRequest request)
        {
            return Ok(_service.Create(request));
        }

        [HttpPost]
        [Route("{id:int}/transition")]
        /// <summary>
        /// Transitions a submission to a new status.
        /// </summary>
        public IHttpActionResult Transition(int id, SubmissionTransitionRequest request)
        {
            return Ok(_service.Transition(id, request.Status));
        }

        [HttpGet]
        [Route("{id:int}/exposure")]
        /// <summary>
        /// Gets submission exposure details.
        /// </summary>
        public IHttpActionResult Exposure(int id)
        {
            return Ok(new { summary = _service.GetExposureSummary(id), records = _service.GetExposureRecords(id) });
        }

        [HttpGet]
        [Route("{id:int}/cat-model")]
        /// <summary>
        /// Gets submission catastrophe model results.
        /// </summary>
        public IHttpActionResult CatModel(int id)
        {
            return Ok(_catModels.GetAll(id));
        }
    }

    public sealed class SubmissionTransitionRequest
    {
        public SubmissionStatus Status { get; set; }
    }
}
