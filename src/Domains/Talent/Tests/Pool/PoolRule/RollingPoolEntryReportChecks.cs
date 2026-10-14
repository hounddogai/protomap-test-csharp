namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class RollingPoolEntryReportChecks
{
    public static void RollingPoolEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703626535);
        var items = new List<RollingPoolEntry>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(RollingPoolEntry.NewRollingPoolEntry($"rolling-pool-entry-{index}", "sample"));
        }
        var report = RollingPoolEntryReport.BuildRollingPoolEntryReport("test", items, clock);
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
        RollingPoolEntryReportCountsEachRecordOnce();
    }
}
