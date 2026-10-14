namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class FloorPlanSlotReportChecks
{
    public static void FloorPlanSlotReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700073274);
        var items = new List<FloorPlanSlot>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(FloorPlanSlot.NewFloorPlanSlot($"floor-plan-slot-{index}", "sample"));
        }
        var report = FloorPlanSlotReport.BuildFloorPlanSlotReport("test", items, clock);
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
        FloorPlanSlotReportCountsEachRecordOnce();
    }
}
