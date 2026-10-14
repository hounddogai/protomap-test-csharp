namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class PrimaryEngagementSummaryReportChecks
{
    public static void PrimaryEngagementSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703969615);
        var items = new List<PrimaryEngagementSummary>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimaryEngagementSummary.NewPrimaryEngagementSummary($"primary-engagement-summary-{index}", "sample"));
        }
        var report = PrimaryEngagementSummaryReport.BuildPrimaryEngagementSummaryReport("test", items, clock);
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
        PrimaryEngagementSummaryReportCountsEachRecordOnce();
    }
}
