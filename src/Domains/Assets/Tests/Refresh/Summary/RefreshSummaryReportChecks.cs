namespace Acme.Workforce.Assets.Tests.Refresh.Summary;

using Acme.Workforce.Assets.Refresh.Summary;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RefreshSummaryReportChecks
{
    public static void RefreshSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705117876);
        var items = new List<RefreshSummary>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RefreshSummary.NewRefreshSummary($"refresh-summary-{index}", "sample"));
        }
        var report = RefreshSummaryReport.BuildRefreshSummaryReport("test", items, clock);
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
        RefreshSummaryReportCountsEachRecordOnce();
    }
}
