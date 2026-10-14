namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolThresholdReportChecks
{
    public static void PoolThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706944585);
        var items = new List<PoolThreshold>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(PoolThreshold.NewPoolThreshold($"pool-threshold-{index}", "sample"));
        }
        var report = PoolThresholdReport.BuildPoolThresholdReport("test", items, clock);
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
        PoolThresholdReportCountsEachRecordOnce();
    }
}
