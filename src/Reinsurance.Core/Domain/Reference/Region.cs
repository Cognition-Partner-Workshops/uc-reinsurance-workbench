using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Reference
{
    [DataContract]
    public partial class Region : BaseEntity
    {
        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string CountryCode { get; set; }

        public virtual ICollection<Exposure.ExposureRecord> ExposureRecords { get; set; } = new List<Exposure.ExposureRecord>();
    }
}
