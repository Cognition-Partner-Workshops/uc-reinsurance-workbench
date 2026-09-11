using System;
using System.Collections.Generic;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Reinsurance.Core.Data;
using Reinsurance.Core.Data.Hooks;
using Reinsurance.Data;
using Reinsurance.Data.Hooks;
using Reinsurance.Services;

namespace Reinsurance.Api
{
    public static class AutofacConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();
            var configuration = GlobalConfiguration.Configuration;
            builder.RegisterApiControllers(typeof(WebApiApplication).Assembly);
            builder.Register(c => new ReinsuranceObjectContext(WebApiApplication.ConnectionString(), c.Resolve<IEnumerable<IDbSaveHook>>())).As<ReinsuranceObjectContext>().As<IDbContext>().InstancePerRequest();
            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerRequest();
            builder.RegisterType<AuditableHook>().As<IDbSaveHook>().InstancePerRequest();
            builder.RegisterType<CedentService>().As<ICedentService>().InstancePerRequest();
            builder.RegisterType<SubmissionService>().As<ISubmissionService>().InstancePerRequest();
            builder.RegisterType<TreatyService>().As<ITreatyService>().InstancePerRequest();
            builder.RegisterType<ReferenceService>().As<IReferenceService>().InstancePerRequest();
            builder.RegisterType<LossHistoryService>().As<ILossHistoryService>().InstancePerRequest();
            var container = builder.Build();
            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}
