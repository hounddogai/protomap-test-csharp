namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyRefreshSummaryReportChecks
{
    public static void LegacyRefreshSummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706358362);
        var items = new List<LegacyRefreshSummary>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(LegacyRefreshSummary.NewLegacyRefreshSummary($"legacy-refresh-summary-{index}", "sample"));
        }
        var report = LegacyRefreshSummaryReport.BuildLegacyRefreshSummaryReport("test", items, clock);
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
        LegacyRefreshSummaryReportCountsEachRecordOnce();
    }
}
