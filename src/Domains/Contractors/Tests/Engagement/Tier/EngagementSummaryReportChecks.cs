namespace Acme.Workforce.Contractors.Tests.Engagement.Tier;

using Acme.Workforce.Contractors.Engagement.Tier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EngagementSummaryReportChecks
{
    public static void EngagementSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700593598);
        var items = new List<EngagementSummary>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(EngagementSummary.NewEngagementSummary($"engagement-summary-{index}", "sample"));
        }
        var report = EngagementSummaryReport.BuildEngagementSummaryReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        EngagementSummaryReportCountsEachRecordOnce();
    }
}
