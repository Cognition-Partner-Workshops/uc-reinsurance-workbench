using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Web;
using System.Web.Http;
using Autofac;
using Reinsurance.Core.Data.Hooks;
using Reinsurance.Data;
using Reinsurance.Data.Hooks;
using Reinsurance.Data.Migrations;
using Reinsurance.Data.Setup;
using Sentry;
using MigrationConfiguration = Reinsurance.Data.Migrations.Configuration;

namespace Reinsurance.Api
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            SentryBootstrap.Initialize();
            AutofacConfig.Register();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ReinsuranceObjectContext, MigrationConfiguration>());
            using (var context = new ReinsuranceObjectContext(ConnectionString(), new IDbSaveHook[] { new AuditableHook() }))
            {
                context.Database.Initialize(false);
                DemoDataSeeder.Seed(context);
            }
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            if (exception != null && SentrySdk.IsEnabled) SentrySdk.CaptureException(exception);
        }

        internal static string ConnectionString()
        {
            var fromEnvironment = Environment.GetEnvironmentVariable("REINSURANCE_DB");
            if (!string.IsNullOrWhiteSpace(fromEnvironment)) return fromEnvironment;
            return ConfigurationManager.ConnectionStrings["ReinsuranceDb"].ConnectionString;
        }
    }

    internal static class SentryBootstrap
    {
        public static void Initialize()
        {
            var dsn = Environment.GetEnvironmentVariable("SENTRY_DSN");
            if (string.IsNullOrWhiteSpace(dsn)) return;
            SentrySdk.Init(o =>
            {
                o.Dsn = dsn;
                o.Environment = Environment.GetEnvironmentVariable("REINSURANCE_ENVIRONMENT") ?? "Development";
                o.Release = Environment.GetEnvironmentVariable("REINSURANCE_RELEASE") ?? "reinsurance-workbench";
            });
        }
    }
}
