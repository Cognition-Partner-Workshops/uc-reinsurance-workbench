using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Cedents
{
    [DataContract]
    public partial class Broker : BaseEntity
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Code { get; set; }

        public virtual ICollection<Submissions.Submission> Submissions { get; set; } = new List<Submissions.Submission>();
    }
}
