using System.Data.Entity.Migrations;

namespace Reinsurance.Data.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<ReinsuranceObjectContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "Reinsurance.Data.ReinsuranceObjectContext";
        }
    }
}
