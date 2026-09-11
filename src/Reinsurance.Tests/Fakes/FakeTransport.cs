using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sentry.Extensibility;
using Sentry.Protocol.Envelopes;

namespace Reinsurance.Tests.Fakes
{
    public sealed class FakeTransport : ITransport
    {
        public IList<Envelope> Envelopes { get; } = new List<Envelope>();

        public Task SendEnvelopeAsync(Envelope envelope, CancellationToken cancellationToken)
        {
            Envelopes.Add(envelope);
            return Task.CompletedTask;
        }
    }
}
