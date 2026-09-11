using System;
using System.Collections.Generic;
using Reinsurance.Core.Domain.Treaties;

namespace Reinsurance.Services.Treaties.Models
{
    public sealed class TreatyCreateRequest
    {
        public int SubmissionId { get; set; }
        public string Name { get; set; }
        public TreatyType Type { get; set; }
        public string Currency { get; set; }
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public IList<TreatyLayerRequest> Layers { get; set; } = new List<TreatyLayerRequest>();
    }
}
