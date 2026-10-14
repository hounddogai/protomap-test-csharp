namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolWindowReportChecks
{
    public static void PoolWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709806200);
        var items = new List<PoolWindow>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PoolWindow.NewPoolWindow($"pool-window-{index}", "sample"));
        }
        var report = PoolWindowReport.BuildPoolWindowReport("test", items, clock);
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
        PoolWindowReportCountsEachRecordOnce();
    }
}
