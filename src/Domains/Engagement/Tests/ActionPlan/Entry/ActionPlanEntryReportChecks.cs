namespace Acme.Workforce.Engagement.Tests.ActionPlan.Entry;

using Acme.Workforce.Engagement.ActionPlan.Entry;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ActionPlanEntryReportChecks
{
    public static void ActionPlanEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706237040);
        var items = new List<ActionPlanEntry>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(ActionPlanEntry.NewActionPlanEntry($"action-plan-entry-{index}", "sample"));
        }
        var report = ActionPlanEntryReport.BuildActionPlanEntryReport("test", items, clock);
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
        ActionPlanEntryReportCountsEachRecordOnce();
    }
}
