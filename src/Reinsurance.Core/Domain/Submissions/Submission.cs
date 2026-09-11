using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Submissions
{
    [DataContract]
    public partial class Submission : BaseEntity, IAuditable, ISoftDeletable
    {
        [DataMember]
        public string Reference { get; set; }

        [DataMember]
        public int CedentId { get; set; }

        [DataMember]
        public int? BrokerId { get; set; }

        [DataMember]
        public int? UnderwriterId { get; set; }

        [DataMember]
        public DateTime ReceivedOn { get; set; }

        [DataMember]
        public DateTime InceptionDate { get; set; }

        [DataMember]
        public DateTime ExpiryDate { get; set; }

        [DataMember]
        public SubmissionStatus Status { get; set; }

        [DataMember]
        public string Notes { get; set; }

        [DataMember]
        public bool Deleted { get; set; }

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;
        public virtual Cedents.Cedent Cedent { get; set; }
        public virtual Cedents.Broker Broker { get; set; }
        public virtual Cedents.Underwriter Underwriter { get; set; }
        public virtual ICollection<Treaties.Treaty> Treaties { get; set; } = new List<Treaties.Treaty>();
        public virtual ICollection<Exposure.ExposureRecord> ExposureRecords { get; set; } = new List<Exposure.ExposureRecord>();
        public virtual ICollection<CatModel.CatModelResult> CatModelResults { get; set; } = new List<CatModel.CatModelResult>();
    }
}
