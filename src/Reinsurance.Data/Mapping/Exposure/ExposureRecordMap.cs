using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Exposure;

namespace Reinsurance.Data.Mapping.Exposure
{
    public partial class ExposureRecordMap : EntityTypeConfiguration<ExposureRecord>
    {
        public ExposureRecordMap()
        {
            ToTable("ExposureRecords");
            HasKey(x => x.Id);
            Property(x => x.TotalInsuredValue).HasPrecision(18, 2);
            Property(x => x.AverageDeductiblePct).HasPrecision(9, 6);
            HasRequired(x => x.Submission).WithMany(x => x.ExposureRecords).HasForeignKey(x => x.SubmissionId).WillCascadeOnDelete(false);
            HasRequired(x => x.Region).WithMany(x => x.ExposureRecords).HasForeignKey(x => x.RegionId).WillCascadeOnDelete(false);
            HasRequired(x => x.Peril).WithMany(x => x.ExposureRecords).HasForeignKey(x => x.PerilId).WillCascadeOnDelete(false);
        }
    }
}
