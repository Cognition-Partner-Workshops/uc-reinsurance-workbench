using System;
using System.Collections.Generic;
using Reinsurance.Core.Domain.Submissions;
using Reinsurance.Services.Treaties.Models;

namespace Reinsurance.Services.Submissions.Models
{
    public sealed class SubmissionModel
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public int CedentId { get; set; }
        public string Cedent { get; set; }
        public string Broker { get; set; }
        public string Underwriter { get; set; }
        public SubmissionStatus Status { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Notes { get; set; }
        public IList<TreatyModel> Treaties { get; set; } = new List<TreatyModel>();
    }
}
