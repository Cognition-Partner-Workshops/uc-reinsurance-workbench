using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.CatModel;

namespace Reinsurance.Data.Mapping.CatModel
{
    public partial class CatModelResultMap : EntityTypeConfiguration<CatModelResult>
    {
        public CatModelResultMap()
        {
            ToTable("CatModelResults");
            HasKey(x => x.Id);
            Property(x => x.ModelVendor).IsRequired().HasMaxLength(100);
            Property(x => x.ModelVersion).IsRequired().HasMaxLength(50);
            Property(x => x.AAL).HasPrecision(18, 2);
            Property(x => x.ExpectedLoss).HasPrecision(18, 2);
            Property(x => x.PML50).HasPrecision(18, 2);
            Property(x => x.PML100).HasPrecision(18, 2);
            Property(x => x.PML250).HasPrecision(18, 2);
            HasRequired(x => x.Submission).WithMany(x => x.CatModelResults).HasForeignKey(x => x.SubmissionId).WillCascadeOnDelete(false);
        }
    }
}
