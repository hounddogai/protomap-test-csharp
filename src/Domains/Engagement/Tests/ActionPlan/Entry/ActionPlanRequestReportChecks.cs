namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanRequestReportChecks
{
    public static void ActionPlanRequestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702495929);
        var items = new List<ActionPlanRequest>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ActionPlanRequest.NewActionPlanRequest($"action-plan-request-{index}", "sample"));
        }
        var report = ActionPlanRequestReport.BuildActionPlanRequestReport("test", items, clock);
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
        ActionPlanRequestReportCountsEachRecordOnce();
    }
}
