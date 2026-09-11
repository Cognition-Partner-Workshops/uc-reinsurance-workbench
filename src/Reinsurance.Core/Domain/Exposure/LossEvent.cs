using System;
using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Exposure
{
    [DataContract]
    public partial class LossEvent : BaseEntity
    {
        [DataMember]
        public int CedentId { get; set; }

        [DataMember]
        public int RegionId { get; set; }

        [DataMember]
        public int PerilId { get; set; }

        [DataMember]
        public string EventName { get; set; }

        [DataMember]
        public DateTime LossDate { get; set; }

        [DataMember]
        public decimal GroundUpLoss { get; set; }

        [DataMember]
        public decimal? CededLoss { get; set; }

        [DataMember]
        public int? TreatyId { get; set; }
    }
}
