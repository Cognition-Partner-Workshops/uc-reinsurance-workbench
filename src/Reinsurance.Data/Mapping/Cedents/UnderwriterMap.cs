using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Cedents;

namespace Reinsurance.Data.Mapping.Cedents
{
    public partial class UnderwriterMap : EntityTypeConfiguration<Underwriter>
    {
        public UnderwriterMap()
        {
            ToTable("Underwriters");
            HasKey(x => x.Id);
            Property(x => x.Name).IsRequired().HasMaxLength(200);
            Property(x => x.Email).IsRequired().HasMaxLength(200);
            Property(x => x.AuthorityLimit).HasPrecision(18, 2);
        }
    }
}
