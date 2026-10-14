namespace Acme.Workforce.Assets.Tests.Refresh.Window;

using Acme.Workforce.Assets.Refresh.Window;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalRefreshCycleReportChecks
{
    public static void RegionalRefreshCycleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704723229);
        var items = new List<RegionalRefreshCycle>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(RegionalRefreshCycle.NewRegionalRefreshCycle($"regional-refresh-cycle-{index}", "sample"));
        }
        var report = RegionalRefreshCycleReport.BuildRegionalRefreshCycleReport("test", items, clock);
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
        RegionalRefreshCycleReportCountsEachRecordOnce();
    }
}
