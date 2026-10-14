namespace Acme.Workforce.Engagement.Tests.ActionPlan.Mapping;

using Acme.Workforce.Engagement.ActionPlan.Mapping;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class ActionPlanMappingReportChecks
{
    public static void ActionPlanMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700196808);
        var items = new List<ActionPlanMapping>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ActionPlanMapping.NewActionPlanMapping($"action-plan-mapping-{index}", "sample"));
        }
        var report = ActionPlanMappingReport.BuildActionPlanMappingReport("test", items, clock);
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
        ActionPlanMappingReportCountsEachRecordOnce();
    }
}
