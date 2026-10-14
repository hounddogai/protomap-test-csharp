namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanRollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.PlanRollup;

/// <summary>Checks of plan rollup code, run by the project's runner.</summary>
public static class PrimarySuccessionPlanRollupReportChecks
{
    public static void PrimarySuccessionPlanRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707087450);
        var items = new List<PrimarySuccessionPlanRollup>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(PrimarySuccessionPlanRollup.NewPrimarySuccessionPlanRollup($"primary-succession-plan-rollup-{index}", "sample"));
        }
        var report = PrimarySuccessionPlanRollupReport.BuildPrimarySuccessionPlanRollupReport("test", items, clock);
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
        PrimarySuccessionPlanRollupReportCountsEachRecordOnce();
    }
}
