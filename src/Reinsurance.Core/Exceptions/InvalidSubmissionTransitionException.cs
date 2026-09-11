using System;
using Reinsurance.Core.Domain.Submissions;

namespace Reinsurance.Core.Exceptions
{
    public sealed class InvalidSubmissionTransitionException : Exception
    {
        public InvalidSubmissionTransitionException(SubmissionStatus current, SubmissionStatus requested)
            : base(string.Format("Submission cannot transition from {0} to {1}.", current, requested))
        {
        }
    }
}
