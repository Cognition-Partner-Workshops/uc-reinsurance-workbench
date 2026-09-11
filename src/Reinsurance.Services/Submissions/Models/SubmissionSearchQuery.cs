using Reinsurance.Core.Domain.Submissions;

namespace Reinsurance.Services.Submissions.Models
{
    public sealed class SubmissionSearchQuery
    {
        public SubmissionStatus? Status { get; set; }
        public int? CedentId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
