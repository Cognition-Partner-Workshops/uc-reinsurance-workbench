using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Cedents
{
    /// <summary>
    /// Represents an insurance cedent.
    /// </summary>
    [DataContract]
    public partial class Cedent : BaseEntity, IAuditable
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Country { get; set; }

        [DataMember]
        public string Rating { get; set; }

        [DataMember]
        public bool Active { get; set; }

        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedOnUtc { get; set; } = DateTime.UtcNow;
        public virtual ICollection<Submissions.Submission> Submissions { get; set; } = new List<Submissions.Submission>();
    }
}
