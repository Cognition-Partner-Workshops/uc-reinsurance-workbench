using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Reinsurance.Tests.Api
{
    /// <summary>
    /// Pins the hosting settings that keep the API from evaluating <c>Request.Browser</c>.
    /// </summary>
    [TestFixture]
    [Category("Unit")]
    public class WebConfigTests
    {
        [Test]
        public void WebPagesRoutingIsExplicitlyDisabled()
        {
            var webConfig = File.ReadAllText(ApiWebConfigPath());
            var setting = Regex.Match(
                webConfig,
                @"<add\s+key=""webpages:Enabled""\s+value=""(?<value>[^""]*)""",
                RegexOptions.IgnoreCase);

            Assert.That(setting.Success, Is.True, "Reinsurance.Api/Web.config must set webpages:Enabled in appSettings.");
            Assert.That(
                setting.Groups["value"].Value,
                Is.EqualTo("false").IgnoreCase,
                "webpages:Enabled must be false so WebPageHttpModule does not match routes and evaluate Request.Browser for API requests.");
        }

        private static string ApiWebConfigPath()
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "Reinsurance.Api", "Web.config");
                if (File.Exists(candidate)) return candidate;
                directory = directory.Parent;
            }
            Assert.Fail("Could not locate Reinsurance.Api/Web.config from " + TestContext.CurrentContext.TestDirectory);
            return null;
        }
    }
}
