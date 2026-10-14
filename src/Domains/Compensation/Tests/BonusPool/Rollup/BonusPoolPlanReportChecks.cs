namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BonusPoolPlanReportChecks
{
    public static void BonusPoolPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707651978);
        var items = new List<BonusPoolPlan>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(BonusPoolPlan.NewBonusPoolPlan($"bonus-pool-plan-{index}", "sample"));
        }
        var report = BonusPoolPlanReport.BuildBonusPoolPlanReport("test", items, clock);
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
        BonusPoolPlanReportCountsEachRecordOnce();
    }
}
