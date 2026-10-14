namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredEquityGrantWindowReportChecks
{
    public static void TieredEquityGrantWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705373915);
        var items = new List<TieredEquityGrantWindow>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredEquityGrantWindow.NewTieredEquityGrantWindow($"tiered-equity-grant-window-{index}", "sample"));
        }
        var report = TieredEquityGrantWindowReport.BuildTieredEquityGrantWindowReport("test", items, clock);
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
        TieredEquityGrantWindowReportCountsEachRecordOnce();
    }
}
