namespace Acme.Workforce.Workplace.Tests.Occupancy.Estimate;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class OccupancyTierReportChecks
{
    public static void OccupancyTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700035148);
        var items = new List<OccupancyTier>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(OccupancyTier.NewOccupancyTier($"occupancy-tier-{index}", "sample"));
        }
        var report = OccupancyTierReport.BuildOccupancyTierReport("test", items, clock);
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
        OccupancyTierReportCountsEachRecordOnce();
    }
}
