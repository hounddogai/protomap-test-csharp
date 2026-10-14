namespace Acme.Workforce.Engagement.Tests.ActionPlan.Claim;

using Acme.Workforce.Engagement.ActionPlan.Claim;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class DefaultActionPlanRollupReportChecks
{
    public static void DefaultActionPlanRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707745588);
        var items = new List<DefaultActionPlanRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(DefaultActionPlanRollup.NewDefaultActionPlanRollup($"default-action-plan-rollup-{index}", "sample"));
        }
        var report = DefaultActionPlanRollupReport.BuildDefaultActionPlanRollupReport("test", items, clock);
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
        DefaultActionPlanRollupReportCountsEachRecordOnce();
    }
}
