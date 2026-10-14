namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackClaimReportChecks
{
    public static void SeasonalFeedbackClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708408842);
        var items = new List<SeasonalFeedbackClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SeasonalFeedbackClaim.NewSeasonalFeedbackClaim($"seasonal-feedback-claim-{index}", "sample"));
        }
        var report = SeasonalFeedbackClaimReport.BuildSeasonalFeedbackClaimReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SeasonalFeedbackClaimReportCountsEachRecordOnce();
    }
}
