using System.Collections.Generic;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Reinsurance.Core.Data;
using Reinsurance.Core.Data.Hooks;
using Reinsurance.Data;
using Reinsurance.Data.Hooks;
using Reinsurance.Services.CatModel;
using Reinsurance.Services.Cedents;
using Reinsurance.Services.Exposure;
using Reinsurance.Services.LossHistory;
using Reinsurance.Services.Portfolio;
using Reinsurance.Services.Pricing;
using Reinsurance.Services.Referrals;
using Reinsurance.Services.Reference;
using Reinsurance.Services.Submissions;
using Reinsurance.Services.Treaties;

namespace Reinsurance.Api.Infrastructure
{
    public sealed class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder)
        {
            builder.RegisterControllers(typeof(WebApiApplication).Assembly);
            builder.RegisterApiControllers(typeof(WebApiApplication).Assembly);
            builder.Register(c => new ReinsuranceObjectContext(WebApiApplication.ConnectionString(), c.Resolve<IEnumerable<IDbSaveHook>>()))
                .As<ReinsuranceObjectContext>().As<IDbContext>().InstancePerRequest();
            builder.RegisterGeneric(typeof(EfRepository<>)).As(typeof(IRepository<>)).InstancePerRequest();
            builder.RegisterType<AuditableHook>().As<IDbSaveHook>().InstancePerRequest();
            builder.RegisterType<CedentService>().As<ICedentService>().InstancePerRequest();
            builder.RegisterType<SubmissionService>().As<ISubmissionService>().InstancePerRequest();
            builder.RegisterType<TreatyService>().As<ITreatyService>().InstancePerRequest();
            builder.RegisterType<ExposureService>().As<IExposureService>().InstancePerRequest();
            builder.RegisterType<CatModelResultService>().As<ICatModelResultService>().InstancePerRequest();
            builder.RegisterType<PricingService>().As<IPricingService>().InstancePerRequest();
            builder.RegisterType<PortfolioService>().As<IPortfolioService>().InstancePerRequest();
            builder.RegisterType<ReferralService>().As<IReferralService>().InstancePerRequest();
            builder.RegisterType<LossHistoryService>().As<ILossHistoryService>().InstancePerRequest();
            builder.RegisterType<ReferenceService>().As<IReferenceService>().InstancePerRequest();
        }
    }
}
