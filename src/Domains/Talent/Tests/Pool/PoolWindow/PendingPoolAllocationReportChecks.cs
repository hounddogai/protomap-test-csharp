namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class PendingPoolAllocationReportChecks
{
    public static void PendingPoolAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702607423);
        var items = new List<PendingPoolAllocation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(PendingPoolAllocation.NewPendingPoolAllocation($"pending-pool-allocation-{index}", "sample"));
        }
        var report = PendingPoolAllocationReport.BuildPendingPoolAllocationReport("test", items, clock);
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
        PendingPoolAllocationReportCountsEachRecordOnce();
    }
}
