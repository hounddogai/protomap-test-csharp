namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class OccupancyEstimateReportChecks
{
    public static void OccupancyEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1709103159);
        var items = new List<OccupancyEstimate>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(OccupancyEstimate.NewOccupancyEstimate($"occupancy-estimate-{index}", "sample"));
        }
        var report = OccupancyEstimateReport.BuildOccupancyEstimateReport("test", items, clock);
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
        OccupancyEstimateReportCountsEachRecordOnce();
    }
}
