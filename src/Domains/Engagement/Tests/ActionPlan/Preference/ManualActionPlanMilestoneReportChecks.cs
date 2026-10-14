namespace Acme.Workforce.Engagement.Tests.ActionPlan.Preference;

using Acme.Workforce.Engagement.ActionPlan.Preference;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ManualActionPlanMilestoneReportChecks
{
    public static void ManualActionPlanMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702096626);
        var items = new List<ManualActionPlanMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ManualActionPlanMilestone.NewManualActionPlanMilestone($"manual-action-plan-milestone-{index}", "sample"));
        }
        var report = ManualActionPlanMilestoneReport.BuildManualActionPlanMilestoneReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ManualActionPlanMilestoneReportCountsEachRecordOnce();
    }
}
