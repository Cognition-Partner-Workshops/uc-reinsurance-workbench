using System;
using Reinsurance.Core.Domain.Submissions;

namespace Reinsurance.Services.Submissions.Models
{
    public sealed class SubmissionListItemModel
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public SubmissionStatus Status { get; set; }
        public int CedentId { get; set; }
        public string CedentName { get; set; }
        public string CedentRating { get; set; }
        public string BrokerName { get; set; }
        public string UnderwriterName { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int TreatyCount { get; set; }
    }
}
