using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Cedents
{
    [DataContract]
    public partial class Underwriter : BaseEntity
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public decimal AuthorityLimit { get; set; }

        [DataMember]
        public bool Active { get; set; }

        public virtual ICollection<Submissions.Submission> Submissions { get; set; } = new List<Submissions.Submission>();
    }
}
