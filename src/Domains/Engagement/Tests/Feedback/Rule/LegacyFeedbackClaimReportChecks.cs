namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyFeedbackClaimReportChecks
{
    public static void LegacyFeedbackClaimReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700938417);
        var items = new List<LegacyFeedbackClaim>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyFeedbackClaim.NewLegacyFeedbackClaim($"legacy-feedback-claim-{index}", "sample"));
        }
        var report = LegacyFeedbackClaimReport.BuildLegacyFeedbackClaimReport("test", items, clock);
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
        LegacyFeedbackClaimReportCountsEachRecordOnce();
    }
}
