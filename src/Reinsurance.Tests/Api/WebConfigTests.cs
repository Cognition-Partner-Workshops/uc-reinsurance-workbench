using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Reinsurance.Tests.Api
{
    [TestFixture]
    public class WebConfigTests
    {
        /// <summary>
        /// ASP.NET WebPages routing evaluates Request.Browser on every request, which on Mono
        /// races inside the browscap loader and fails API calls with IndexOutOfRangeException.
        /// The host serves no WebPages content, so routing must stay explicitly disabled.
        /// </summary>
        [Test]
        public void WebPagesRoutingIsExplicitlyDisabled()
        {
            var webConfig = File.ReadAllText(WebConfigPath());
            var enabled = Regex.Match(webConfig, "<add\\s+key=\"webpages:Enabled\"\\s+value=\"([^\"]*)\"");

            Assert.That(enabled.Success, Is.True, "Expected appSettings key webpages:Enabled in Reinsurance.Api/Web.config");
            Assert.That(enabled.Groups[1].Value, Is.EqualTo("false").IgnoreCase);
        }

        private static string WebConfigPath()
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "Reinsurance.Api", "Web.config");
                if (File.Exists(candidate))
                    return candidate;
                directory = directory.Parent;
            }
            throw new FileNotFoundException("Reinsurance.Api/Web.config was not found above " + TestContext.CurrentContext.TestDirectory);
        }
    }
}
