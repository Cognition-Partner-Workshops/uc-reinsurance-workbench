using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Treaties
{
    [DataContract]
    public partial class Treaty : BaseEntity, IAuditable
    {
        [DataMember]
        public int SubmissionId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public TreatyType Type { get; set; }

        [DataMember]
        public string Currency { get; set; }

        [DataMember]
        public TreatyStatus Status { get; set; }

        [DataMember]
        public DateTime InceptionDate { get; set; }

        [DataMember]
        public DateTime ExpiryDate { get; set; }

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;
        public virtual Submissions.Submission Submission { get; set; }
        public virtual ICollection<TreatyLayer> Layers { get; set; } = new List<TreatyLayer>();
    }
}
