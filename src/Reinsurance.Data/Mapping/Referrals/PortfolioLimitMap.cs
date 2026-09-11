using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Referrals;

namespace Reinsurance.Data.Mapping.Referrals
{
    public partial class PortfolioLimitMap : EntityTypeConfiguration<PortfolioLimit>
    {
        public PortfolioLimitMap()
        {
            ToTable("PortfolioLimits");
            HasKey(x => x.Id);
            Property(x => x.MaxAggregateLimit).HasPrecision(18, 2);
            Property(x => x.MaxPML250).HasPrecision(18, 2);
            Property(x => x.Description).IsRequired().HasMaxLength(500);
        }
    }
}
