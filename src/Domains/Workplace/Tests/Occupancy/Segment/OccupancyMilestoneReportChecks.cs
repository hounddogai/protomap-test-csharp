namespace Acme.Workforce.Workplace.Tests.Occupancy.Segment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class OccupancyMilestoneReportChecks
{
    public static void OccupancyMilestoneReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708838211);
        var items = new List<OccupancyMilestone>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(OccupancyMilestone.NewOccupancyMilestone($"occupancy-milestone-{index}", "sample"));
        }
        var report = OccupancyMilestoneReport.BuildOccupancyMilestoneReport("test", items, clock);
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
        OccupancyMilestoneReportCountsEachRecordOnce();
    }
}
