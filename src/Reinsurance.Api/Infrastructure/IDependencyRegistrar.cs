using Autofac;

namespace Reinsurance.Api.Infrastructure
{
    public interface IDependencyRegistrar
    {
        void Register(ContainerBuilder builder);
    }
}
