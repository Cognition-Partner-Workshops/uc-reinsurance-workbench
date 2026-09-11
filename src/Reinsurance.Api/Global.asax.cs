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
using Sentry.AspNet;
using Sentry.Extensibility;
using Sentry.Infrastructure;
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
            RouteConfig.Register(System.Web.Routing.RouteTable.Routes);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ReinsuranceObjectContext, MigrationConfiguration>());
            using (var context = new ReinsuranceObjectContext(ConnectionString(), new IDbSaveHook[] { new AuditableHook() }))
            {
                context.Database.Initialize(false);
                DemoDataSeeder.Seed(context);
            }
        }

        protected void Application_Error()
        {
            if (SentrySdk.IsEnabled) Context.Server.CaptureLastError();
        }

        protected void Application_BeginRequest()
        {
            if (SentrySdk.IsEnabled) Context.StartSentryTransaction();
        }

        protected void Application_EndRequest()
        {
            if (SentrySdk.IsEnabled) Context.FinishSentryTransaction();
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
                o.AddAspNet(RequestSize.Small);
                o.TracesSampleRate = 1.0;
                o.SendDefaultPii = false;
                o.AttachStacktrace = true;
                var debugLog = Environment.GetEnvironmentVariable("SENTRY_DEBUG_LOG");
                if (!string.IsNullOrWhiteSpace(debugLog))
                {
                    o.Debug = true;
                    o.DiagnosticLogger = new FileDiagnosticLogger(debugLog, SentryLevel.Debug);
                }
                o.Environment = Environment.GetEnvironmentVariable("REINSURANCE_ENVIRONMENT") ?? "Development";
                o.Release = Environment.GetEnvironmentVariable("REINSURANCE_RELEASE")
                    ?? "reinsurance-workbench@" + typeof(WebApiApplication).Assembly.GetName().Version;
            });
        }
    }
}
