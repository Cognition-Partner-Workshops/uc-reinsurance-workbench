using System;
using System.Collections.Generic;
using Reinsurance.Core.Domain.Treaties;

namespace Reinsurance.Services.Treaties.Models
{
    public sealed class TreatyModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TreatyType Type { get; set; }
        public TreatyStatus Status { get; set; }
        public string Currency { get; set; }
        public int SubmissionId { get; set; }
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public IList<TreatyLayerModel> Layers { get; set; } = new List<TreatyLayerModel>();
    }
}
