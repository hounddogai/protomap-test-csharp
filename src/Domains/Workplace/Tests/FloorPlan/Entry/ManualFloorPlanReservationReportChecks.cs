namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualFloorPlanReservationReportChecks
{
    public static void ManualFloorPlanReservationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705773082);
        var items = new List<ManualFloorPlanReservation>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ManualFloorPlanReservation.NewManualFloorPlanReservation($"manual-floor-plan-reservation-{index}", "sample"));
        }
        var report = ManualFloorPlanReservationReport.BuildManualFloorPlanReservationReport("test", items, clock);
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
        ManualFloorPlanReservationReportCountsEachRecordOnce();
    }
}
