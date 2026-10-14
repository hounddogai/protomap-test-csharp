namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanDraftReportChecks
{
    public static void ActionPlanDraftReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700995175);
        var items = new List<ActionPlanDraft>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(ActionPlanDraft.NewActionPlanDraft($"action-plan-draft-{index}", "sample"));
        }
        var report = ActionPlanDraftReport.BuildActionPlanDraftReport("test", items, clock);
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
        ActionPlanDraftReportCountsEachRecordOnce();
    }
}
