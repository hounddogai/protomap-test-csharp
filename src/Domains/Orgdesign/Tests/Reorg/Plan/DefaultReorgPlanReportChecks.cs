namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultReorgPlanReportChecks
{
    public static void DefaultReorgPlanReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1704285204);
        var items = new List<DefaultReorgPlan>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(DefaultReorgPlan.NewDefaultReorgPlan($"default-reorg-plan-{index}", "sample"));
        }
        var report = DefaultReorgPlanReport.BuildDefaultReorgPlanReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        DefaultReorgPlanReportCountsEachRecordOnce();
    }
}
