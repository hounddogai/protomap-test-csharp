namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class FallbackBonusPoolMilestoneReportChecks
{
    public static void FallbackBonusPoolMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700671542);
        var items = new List<FallbackBonusPoolMilestone>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone($"fallback-bonus-pool-milestone-{index}", "sample"));
        }
        var report = FallbackBonusPoolMilestoneReport.BuildFallbackBonusPoolMilestoneReport("test", items, clock);
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
        FallbackBonusPoolMilestoneReportCountsEachRecordOnce();
    }
}
