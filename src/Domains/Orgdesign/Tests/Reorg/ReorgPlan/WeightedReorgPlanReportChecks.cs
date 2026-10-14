namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class WeightedReorgPlanReportChecks
{
    public static void WeightedReorgPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702647658);
        var items = new List<WeightedReorgPlan>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(WeightedReorgPlan.NewWeightedReorgPlan($"weighted-reorg-plan-{index}", "sample"));
        }
        var report = WeightedReorgPlanReport.BuildWeightedReorgPlanReport("test", items, clock);
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
        WeightedReorgPlanReportCountsEachRecordOnce();
    }
}
