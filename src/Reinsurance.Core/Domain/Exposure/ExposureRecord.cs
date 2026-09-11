using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.Exposure
{
    [DataContract]
    public partial class ExposureRecord : BaseEntity
    {
        [DataMember]
        public int SubmissionId { get; set; }

        [DataMember]
        public int RegionId { get; set; }

        [DataMember]
        public int PerilId { get; set; }

        [DataMember]
        public decimal TotalInsuredValue { get; set; }

        [DataMember]
        public int RiskCount { get; set; }

        [DataMember]
        public decimal AverageDeductiblePct { get; set; }

        public virtual Submissions.Submission Submission { get; set; }
        public virtual Reference.Region Region { get; set; }
        public virtual Reference.Peril Peril { get; set; }
    }
}
