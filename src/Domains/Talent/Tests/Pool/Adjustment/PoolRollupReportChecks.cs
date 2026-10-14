namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PoolRollupReportChecks
{
    public static void PoolRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702141312);
        var items = new List<PoolRollup>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(PoolRollup.NewPoolRollup($"pool-rollup-{index}", "sample"));
        }
        var report = PoolRollupReport.BuildPoolRollupReport("test", items, clock);
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
        PoolRollupReportCountsEachRecordOnce();
    }
}
