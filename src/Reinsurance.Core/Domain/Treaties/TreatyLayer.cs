using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Treaties
{
    [DataContract]
    public partial class TreatyLayer : BaseEntity
    {
        [DataMember]
        public int TreatyId { get; set; }

        [DataMember]
        public int LayerNumber { get; set; }

        [DataMember]
        public decimal Limit { get; set; }

        [DataMember]
        public decimal Attachment { get; set; }

        [DataMember]
        public int Reinstatements { get; set; }

        [DataMember]
        public decimal ReinstatementPremiumPct { get; set; }

        [DataMember]
        public decimal SharePct { get; set; }

        [DataMember]
        public string Currency { get; set; }

        public virtual Treaty Treaty { get; set; }
    }
}
