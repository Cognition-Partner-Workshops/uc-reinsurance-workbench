using System.Web.Mvc;
using System.Web.Http;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Reinsurance.Api.Infrastructure;

namespace Reinsurance.Api
{
    public static class AutofacConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();
            new DependencyRegistrar().Register(builder);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}
