using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Referrals
{
    [DataContract]
    public partial class PortfolioLimit : BaseEntity
    {
        [DataMember]
        public int? RegionId { get; set; }

        [DataMember]
        public int? PerilId { get; set; }

        [DataMember]
        public decimal MaxAggregateLimit { get; set; }

        [DataMember]
        public decimal MaxPML250 { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}
