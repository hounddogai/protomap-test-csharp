namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ManualActionPlanEntryReportChecks
{
    public static void ManualActionPlanEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708860152);
        var items = new List<ManualActionPlanEntry>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualActionPlanEntry.NewManualActionPlanEntry($"manual-action-plan-entry-{index}", "sample"));
        }
        var report = ManualActionPlanEntryReport.BuildManualActionPlanEntryReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 3, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        ManualActionPlanEntryReportCountsEachRecordOnce();
    }
}
