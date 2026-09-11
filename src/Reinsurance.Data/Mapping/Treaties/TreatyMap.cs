using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Treaties;

namespace Reinsurance.Data.Mapping.Treaties
{
    public partial class TreatyMap : EntityTypeConfiguration<Treaty>
    {
        public TreatyMap()
        {
            ToTable("Treaties");
            HasKey(x => x.Id);
            Property(x => x.Name).IsRequired().HasMaxLength(250);
            Property(x => x.Currency).IsRequired().HasMaxLength(10);
            HasRequired(x => x.Submission).WithMany(x => x.Treaties).HasForeignKey(x => x.SubmissionId).WillCascadeOnDelete(false);
        }
    }
}
