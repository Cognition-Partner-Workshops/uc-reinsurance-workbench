using System;

namespace Reinsurance.Services.Submissions.Models
{
    public sealed class SubmissionCreateRequest
    {
        public int CedentId { get; set; }
        public int? BrokerId { get; set; }
        public int? UnderwriterId { get; set; }
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Notes { get; set; }
    }
}
