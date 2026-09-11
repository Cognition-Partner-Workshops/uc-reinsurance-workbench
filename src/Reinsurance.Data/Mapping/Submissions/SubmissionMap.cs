using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Submissions;

namespace Reinsurance.Data.Mapping.Submissions
{
    public partial class SubmissionMap : EntityTypeConfiguration<Submission>
    {
        public SubmissionMap()
        {
            ToTable("Submissions");
            HasKey(x => x.Id);
            Property(x => x.Reference).IsRequired().HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UX_Submissions_Reference") { IsUnique = true }));
            Property(x => x.Notes).HasMaxLength(2000);
            HasRequired(x => x.Cedent).WithMany(x => x.Submissions).HasForeignKey(x => x.CedentId).WillCascadeOnDelete(false);
            HasOptional(x => x.Broker).WithMany(x => x.Submissions).HasForeignKey(x => x.BrokerId).WillCascadeOnDelete(false);
            HasOptional(x => x.Underwriter).WithMany(x => x.Submissions).HasForeignKey(x => x.UnderwriterId).WillCascadeOnDelete(false);
        }
    }
}
