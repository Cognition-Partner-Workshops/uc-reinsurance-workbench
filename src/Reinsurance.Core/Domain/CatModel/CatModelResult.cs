using System;
using System.Runtime.Serialization;

namespace Reinsurance.Core.Domain.CatModel
{
    [DataContract]
    public partial class CatModelResult : BaseEntity
    {
        [DataMember]
        public int SubmissionId { get; set; }

        [DataMember]
        public int? RegionId { get; set; }

        [DataMember]
        public int? PerilId { get; set; }

        [DataMember]
        public string ModelVendor { get; set; }

        [DataMember]
        public string ModelVersion { get; set; }

        [DataMember]
        public decimal AAL { get; set; }

        [DataMember]
        public decimal ExpectedLoss { get; set; }

        [DataMember]
        public decimal PML50 { get; set; }

        [DataMember]
        public decimal PML100 { get; set; }

        [DataMember]
        public decimal PML250 { get; set; }

        [DataMember]
        public DateTime RunOn { get; set; }

        public virtual Submissions.Submission Submission { get; set; }
    }
}
