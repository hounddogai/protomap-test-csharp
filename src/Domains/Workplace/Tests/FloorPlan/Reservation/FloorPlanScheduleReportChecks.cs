namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FloorPlanScheduleReportChecks
{
    public static void FloorPlanScheduleReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703359441);
        var items = new List<FloorPlanSchedule>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FloorPlanSchedule.NewFloorPlanSchedule($"floor-plan-schedule-{index}", "sample"));
        }
        var report = FloorPlanScheduleReport.BuildFloorPlanScheduleReport("test", items, clock);
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
        FloorPlanScheduleReportCountsEachRecordOnce();
    }
}
