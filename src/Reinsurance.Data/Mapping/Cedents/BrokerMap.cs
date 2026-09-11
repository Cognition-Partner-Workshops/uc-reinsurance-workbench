using System.Data.Entity.ModelConfiguration;
using Reinsurance.Core.Domain.Cedents;

namespace Reinsurance.Data.Mapping.Cedents
{
    public partial class BrokerMap : EntityTypeConfiguration<Broker>
    {
        public BrokerMap()
        {
            ToTable("Brokers");
            HasKey(x => x.Id);
            Property(x => x.Name).IsRequired().HasMaxLength(200);
            Property(x => x.Code).IsRequired().HasMaxLength(50);
        }
    }
}
