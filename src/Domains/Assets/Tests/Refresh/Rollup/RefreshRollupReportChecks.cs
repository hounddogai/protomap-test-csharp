namespace Acme.Workforce.Assets.Tests.Refresh.Rollup;

using Acme.Workforce.Assets.Refresh.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RefreshRollupReportChecks
{
    public static void RefreshRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704034883);
        var items = new List<RefreshRollup>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RefreshRollup.NewRefreshRollup($"refresh-rollup-{index}", "sample"));
        }
        var report = RefreshRollupReport.BuildRefreshRollupReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        RefreshRollupReportCountsEachRecordOnce();
    }
}
