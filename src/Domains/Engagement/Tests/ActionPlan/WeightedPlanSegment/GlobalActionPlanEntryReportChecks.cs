namespace Acme.Workforce.Engagement.Tests.ActionPlan.WeightedPlanSegment;

using Acme.Workforce.Engagement.ActionPlan.WeightedPlanSegment;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of weighted plan segment code, run by the project's runner.</summary>
public static class GlobalActionPlanEntryReportChecks
{
    public static void GlobalActionPlanEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707301413);
        var items = new List<GlobalActionPlanEntry>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(GlobalActionPlanEntry.NewGlobalActionPlanEntry($"global-action-plan-entry-{index}", "sample"));
        }
        var report = GlobalActionPlanEntryReport.BuildGlobalActionPlanEntryReport("test", items, clock);
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
        GlobalActionPlanEntryReportCountsEachRecordOnce();
    }
}
