namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAssignmentReportChecks
{
    public static void OccupancyAssignmentReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706888603);
        var items = new List<OccupancyAssignment>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(OccupancyAssignment.NewOccupancyAssignment($"occupancy-assignment-{index}", "sample"));
        }
        var report = OccupancyAssignmentReport.BuildOccupancyAssignmentReport("test", items, clock);
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
        OccupancyAssignmentReportCountsEachRecordOnce();
    }
}
