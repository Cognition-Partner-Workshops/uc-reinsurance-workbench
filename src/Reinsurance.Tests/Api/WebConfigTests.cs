using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace Reinsurance.Tests.Api
{
    [TestFixture]
    public class WebConfigTests
    {
        /// <summary>
        /// The WebPages HTTP module probes Request.Browser on every request to pick a display mode.
        /// Under Mono that probe goes through the browscap.ini loader, which appends to a shared
        /// ArrayList without synchronisation and throws IndexOutOfRangeException under concurrent
        /// first-seen user agents (BACKEND-REINSURANCE-DEMO-5). The API serves no Razor pages, so
        /// WebPages must stay explicitly disabled.
        /// </summary>
        [Test]
        public void ApiWebConfigExplicitlyDisablesWebPages()
        {
            var appSettings = XDocument.Load(ApiWebConfigPath())
                .Root
                .Element("appSettings")
                .Elements("add")
                .ToDictionary(e => (string)e.Attribute("key"), e => (string)e.Attribute("value"), StringComparer.OrdinalIgnoreCase);

            Assert.That(appSettings.ContainsKey("webpages:Enabled"), Is.True, "webpages:Enabled must be set explicitly in Reinsurance.Api/Web.config");
            Assert.That(bool.Parse(appSettings["webpages:Enabled"]), Is.False);
        }

        private static string ApiWebConfigPath()
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "Reinsurance.Api", "Web.config");
                if (File.Exists(candidate))
                    return candidate;
                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not locate Reinsurance.Api/Web.config above " + TestContext.CurrentContext.TestDirectory);
        }
    }
}
