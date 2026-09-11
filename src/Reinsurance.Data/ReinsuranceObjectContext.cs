using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using Reinsurance.Core.Data.Hooks;

namespace Reinsurance.Data
{
    public class ReinsuranceObjectContext : ObjectContextBase
    {
        public ReinsuranceObjectContext()
            : this("name=ReinsuranceDb", new IDbSaveHook[0])
        {
        }

        public ReinsuranceObjectContext(string connectionString)
            : this(connectionString, new IDbSaveHook[0])
        {
        }

        public ReinsuranceObjectContext(string connectionString, System.Collections.Generic.IEnumerable<IDbSaveHook> hooks)
            : base(connectionString, hooks)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            var configurationType = typeof(EntityTypeConfiguration<>);
            foreach (var type in typeof(ReinsuranceObjectContext).Assembly.GetTypes()
                .Where(x => !x.IsAbstract && FindConfigurationType(x, configurationType) != null))
            {
                var configuration = Activator.CreateInstance(type);
                var entityType = FindConfigurationType(type, configurationType).GetGenericArguments()[0];
                var add = typeof(ConfigurationRegistrar).GetMethods()
                    .Single(x => x.Name == "Add" && x.IsGenericMethodDefinition &&
                                 x.GetParameters().Length == 1 &&
                                 x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == configurationType)
                    .MakeGenericMethod(entityType);
                add.Invoke(modelBuilder.Configurations, new[] { configuration });
            }
            base.OnModelCreating(modelBuilder);
        }

        private static Type FindConfigurationType(Type type, Type configurationType)
        {
            for (var current = type.BaseType; current != null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == configurationType)
                    return current;
            }
            return null;
        }
    }
}
