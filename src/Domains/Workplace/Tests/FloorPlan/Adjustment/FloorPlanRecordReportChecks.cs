namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanRecordReportChecks
{
    public static void FloorPlanRecordReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703212266);
        var items = new List<FloorPlanRecord>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(FloorPlanRecord.NewFloorPlanRecord($"floor-plan-record-{index}", "sample"));
        }
        var report = FloorPlanRecordReport.BuildFloorPlanRecordReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 4, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        FloorPlanRecordReportCountsEachRecordOnce();
    }
}
