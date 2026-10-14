namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class LegacyBonusPoolAllocationReportChecks
{
    public static void LegacyBonusPoolAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706568985);
        var items = new List<LegacyBonusPoolAllocation>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(LegacyBonusPoolAllocation.NewLegacyBonusPoolAllocation($"legacy-bonus-pool-allocation-{index}", "sample"));
        }
        var report = LegacyBonusPoolAllocationReport.BuildLegacyBonusPoolAllocationReport("test", items, clock);
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
        LegacyBonusPoolAllocationReportCountsEachRecordOnce();
    }
}
