using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Referrals
{
    [DataContract]
    public partial class ReferralRule : BaseEntity
    {
        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public ReferralRuleKind Kind { get; set; }

        [DataMember]
        public decimal? Threshold { get; set; }

        [DataMember]
        public string TextValue { get; set; }
    }
}
