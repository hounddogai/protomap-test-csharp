namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolMilestoneReportChecks
{
    public static void BonusPoolMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706758601);
        var items = new List<BonusPoolMilestone>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(BonusPoolMilestone.NewBonusPoolMilestone($"bonus-pool-milestone-{index}", "sample"));
        }
        var report = BonusPoolMilestoneReport.BuildBonusPoolMilestoneReport("test", items, clock);
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
        BonusPoolMilestoneReportCountsEachRecordOnce();
    }
}
