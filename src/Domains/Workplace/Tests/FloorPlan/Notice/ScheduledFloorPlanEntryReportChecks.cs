namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledFloorPlanEntryReportChecks
{
    public static void ScheduledFloorPlanEntryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702851120);
        var items = new List<ScheduledFloorPlanEntry>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(ScheduledFloorPlanEntry.NewScheduledFloorPlanEntry($"scheduled-floor-plan-entry-{index}", "sample"));
        }
        var report = ScheduledFloorPlanEntryReport.BuildScheduledFloorPlanEntryReport("test", items, clock);
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
        ScheduledFloorPlanEntryReportCountsEachRecordOnce();
    }
}
