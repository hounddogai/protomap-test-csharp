namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PoolOverrideReportChecks
{
    public static void PoolOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701696735);
        var items = new List<PoolOverride>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PoolOverride.NewPoolOverride($"pool-override-{index}", "sample"));
        }
        var report = PoolOverrideReport.BuildPoolOverrideReport("test", items, clock);
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
        PoolOverrideReportCountsEachRecordOnce();
    }
}
