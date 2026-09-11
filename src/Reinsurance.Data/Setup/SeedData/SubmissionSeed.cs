using System;
using Reinsurance.Core.Domain.Submissions;

namespace Reinsurance.Data.Setup.SeedData
{
    public static class SubmissionSeed
    {
        public static void Seed(ReinsuranceObjectContext context, SeedState state)
        {
            var statuses = new[]
            {
                SubmissionStatus.Received, SubmissionStatus.Received, SubmissionStatus.Received,
                SubmissionStatus.InReview, SubmissionStatus.InReview, SubmissionStatus.Quoted,
                SubmissionStatus.Quoted, SubmissionStatus.Bound, SubmissionStatus.Bound,
                SubmissionStatus.Declined
            };
            for (var i = 0; i < statuses.Length; i++)
            {
                state.Submissions.Add(new Submission
                {
                    Reference = "SUB-2026-" + (i + 1).ToString("0000"),
                    CedentId = state.Cedents[i % state.Cedents.Count].Id,
                    BrokerId = state.Brokers[i % state.Brokers.Count].Id,
                    UnderwriterId = state.Underwriters[i % state.Underwriters.Count].Id,
                    ReceivedOn = new DateTime(2026, 1, 5).AddDays(i),
                    InceptionDate = new DateTime(2026, 1, 1),
                    ExpiryDate = new DateTime(2026, 12, 31),
                    Status = statuses[i],
                    Notes = "2026 property catastrophe placement submission.",
                    Deleted = false
                });
            }
            context.Set<Submission>().AddRange(state.Submissions);
            context.SaveChanges();
        }
    }
}
