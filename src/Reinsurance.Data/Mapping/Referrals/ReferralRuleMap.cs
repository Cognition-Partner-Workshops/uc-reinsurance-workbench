using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Referrals;

namespace Reinsurance.Data.Mapping.Referrals
{
    public partial class ReferralRuleMap : EntityTypeConfiguration<ReferralRule>
    {
        public ReferralRuleMap()
        {
            ToTable("ReferralRules");
            HasKey(x => x.Id);
            Property(x => x.Code).IsRequired().HasMaxLength(100)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UX_ReferralRules_Code") { IsUnique = true }));
            Property(x => x.Description).IsRequired().HasMaxLength(500);
            Property(x => x.Threshold).HasPrecision(18, 2);
            Property(x => x.TextValue).HasMaxLength(200);
        }
    }
}
