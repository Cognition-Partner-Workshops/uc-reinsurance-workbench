using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Reference;

namespace Reinsurance.Data.Mapping.Reference
{
    public partial class RegionMap : EntityTypeConfiguration<Region>
    {
        public RegionMap()
        {
            ToTable("Regions");
            HasKey(x => x.Id);
            Property(x => x.Code).IsRequired().HasMaxLength(30);
            Property(x => x.Name).IsRequired().HasMaxLength(200);
            Property(x => x.CountryCode).IsRequired().HasMaxLength(10);
        }
    }
}
