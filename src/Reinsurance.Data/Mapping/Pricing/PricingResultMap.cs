using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Pricing;

namespace Reinsurance.Data.Mapping.Pricing
{
    public partial class PricingResultMap : EntityTypeConfiguration<PricingResult>
    {
        public PricingResultMap()
        {
            ToTable("PricingResults");
            HasKey(x => x.Id);
            Property(x => x.TechnicalPremium).HasPrecision(18, 2);
            Property(x => x.ExpectedLoss).HasPrecision(18, 2);
            Property(x => x.ExpenseLoad).HasPrecision(18, 2);
            Property(x => x.RiskLoad).HasPrecision(18, 2);
            Property(x => x.RateOnLine).HasPrecision(9, 6);
            Property(x => x.LossCostPct).HasPrecision(9, 6);
            Property(x => x.ProfitMarginPct).HasPrecision(9, 6);
            Property(x => x.ReferralReasons).HasMaxLength(2000);
            HasRequired(x => x.TreatyLayer).WithMany().HasForeignKey(x => x.TreatyLayerId).WillCascadeOnDelete(false);
        }
    }
}
