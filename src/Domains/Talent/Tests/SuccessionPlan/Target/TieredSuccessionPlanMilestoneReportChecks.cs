namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Target;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.SuccessionPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class TieredSuccessionPlanMilestoneReportChecks
{
    public static void TieredSuccessionPlanMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705444176);
        var items = new List<TieredSuccessionPlanMilestone>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(TieredSuccessionPlanMilestone.NewTieredSuccessionPlanMilestone($"tiered-succession-plan-milestone-{index}", "sample"));
        }
        var report = TieredSuccessionPlanMilestoneReport.BuildTieredSuccessionPlanMilestoneReport("test", items, clock);
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
        TieredSuccessionPlanMilestoneReportCountsEachRecordOnce();
    }
}
