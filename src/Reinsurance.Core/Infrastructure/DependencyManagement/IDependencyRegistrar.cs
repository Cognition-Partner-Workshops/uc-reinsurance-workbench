using Autofac;

namespace Reinsurance.Core.Infrastructure.DependencyManagement
{
    public interface IDependencyRegistrar
    {
        void Register(ContainerBuilder builder);
        int Order { get; }
    }
}
