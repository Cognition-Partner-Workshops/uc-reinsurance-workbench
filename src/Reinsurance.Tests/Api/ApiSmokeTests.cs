using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Reinsurance.Tests.Api
{
    [TestFixture]
    [Category("Integration")]
    public class ApiSmokeTests
    {
        private HttpClient _client;

        [SetUp]
        public void SetUp()
        {
            _client = new HttpClient { BaseAddress = new Uri(Environment.GetEnvironmentVariable("REINSURANCE_API_URL") ?? "http://localhost:5055") };
            _client.Timeout = TimeSpan.FromSeconds(3);
            try
            {
                var response = _client.GetAsync("/api/health").GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                    Assert.Ignore("The API health endpoint is unavailable.");
            }
            catch (Exception)
            {
                Assert.Ignore("The API health endpoint is unavailable.");
            }
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
        }

        [Test]
        public void HealthReturnsOk()
        {
            var response = _client.GetAsync("/api/health").GetAwaiter().GetResult();
            var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Does.Contain("\"status\":\"ok\""));
        }

        [Test]
        public void SubmissionsContainSeededReferences()
        {
            var body = Get("/api/submissions");
            var references = Regex.Matches(body, "SUB-2026-\\d{4}");

            Assert.That(references.Count, Is.GreaterThanOrEqualTo(10));
            Assert.That(body, Does.Contain("\"id\":"));
            Assert.That(body, Does.Contain("\"cedentName\":"));
        }

        [Test]
        public void BoundFilterReturnsOnlyBoundItems()
        {
            var body = Get("/api/submissions?status=Bound");
            var statuses = Regex.Matches(body, "\"status\":(\\d+)");

            Assert.That(statuses.Count, Is.GreaterThan(0));
            foreach (Match status in statuses)
                Assert.That(status.Groups[1].Value, Is.EqualTo("3"));
        }

        [Test]
        public void ExposureSummaryHasTiv()
        {
            var body = Get("/api/submissions/1/exposure");

            Assert.That(DecimalValue(body, "totalTiv"), Is.GreaterThan(0m));
        }

        [Test]
        public void HealthyTreatyPricingReturnsRateOnLine()
        {
            var response = _client.PostAsync("/api/treaties/1/price", new StringContent(string.Empty, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
            var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(DecimalValue(body, "rateOnLine"), Is.InRange(0.01m, 0.5m));
        }

        [Test]
        public void IllegalTransitionReturnsConflict()
        {
            var response = _client.PostAsync(
                "/api/submissions/1/transition",
                new StringContent("{\"status\":\"Bound\"}", Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        }

        private string Get(string path)
        {
            return _client.GetStringAsync(path).GetAwaiter().GetResult();
        }

        private static decimal DecimalValue(string body, string property)
        {
            var match = Regex.Match(body, "\"" + property + "\":([0-9.]+)");
            Assert.That(match.Success, Is.True, "Expected JSON property " + property);
            return decimal.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
