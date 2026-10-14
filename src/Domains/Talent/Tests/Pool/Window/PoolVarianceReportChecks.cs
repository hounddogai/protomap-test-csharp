namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolVarianceReportChecks
{
    public static void PoolVarianceReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707060684);
        var items = new List<PoolVariance>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PoolVariance.NewPoolVariance($"pool-variance-{index}", "sample"));
        }
        var report = PoolVarianceReport.BuildPoolVarianceReport("test", items, clock);
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
        PoolVarianceReportCountsEachRecordOnce();
    }
}
