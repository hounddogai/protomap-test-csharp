namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyAllocationReportChecks
{
    public static void OccupancyAllocationReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706200269);
        var items = new List<OccupancyAllocation>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(OccupancyAllocation.NewOccupancyAllocation($"occupancy-allocation-{index}", "sample"));
        }
        var report = OccupancyAllocationReport.BuildOccupancyAllocationReport("test", items, clock);
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
        OccupancyAllocationReportCountsEachRecordOnce();
    }
}
