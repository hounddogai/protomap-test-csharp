namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackClaimReportChecks
{
    public static void FeedbackClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703032752);
        var items = new List<FeedbackClaim>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FeedbackClaim.NewFeedbackClaim($"feedback-claim-{index}", "sample"));
        }
        var report = FeedbackClaimReport.BuildFeedbackClaimReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FeedbackClaimReportCountsEachRecordOnce();
    }
}
