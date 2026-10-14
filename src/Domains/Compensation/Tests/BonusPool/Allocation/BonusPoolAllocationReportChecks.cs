namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolAllocationReportChecks
{
    public static void BonusPoolAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706470358);
        var items = new List<BonusPoolAllocation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BonusPoolAllocation.NewBonusPoolAllocation($"bonus-pool-allocation-{index}", "sample"));
        }
        var report = BonusPoolAllocationReport.BuildBonusPoolAllocationReport("test", items, clock);
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
        BonusPoolAllocationReportCountsEachRecordOnce();
    }
}
