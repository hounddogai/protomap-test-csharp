namespace Acme.Workforce.Contractors.Tests.Engagement.Adjustment;

using Acme.Workforce.Contractors.Engagement.Adjustment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EngagementScoreReportChecks
{
    public static void EngagementScoreReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706906377);
        var items = new List<EngagementScore>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(EngagementScore.NewEngagementScore($"engagement-score-{index}", "sample"));
        }
        var report = EngagementScoreReport.BuildEngagementScoreReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        EngagementScoreReportCountsEachRecordOnce();
    }
}
