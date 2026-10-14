namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class ScheduledPoolWindowReportChecks
{
    public static void ScheduledPoolWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708239698);
        var items = new List<ScheduledPoolWindow>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ScheduledPoolWindow.NewScheduledPoolWindow($"scheduled-pool-window-{index}", "sample"));
        }
        var report = ScheduledPoolWindowReport.BuildScheduledPoolWindowReport("test", items, clock);
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
        ScheduledPoolWindowReportCountsEachRecordOnce();
    }
}
