namespace Acme.Workforce.Workplace.Tests.Occupancy.Summary;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class OccupancySummaryReportChecks
{
    public static void OccupancySummaryReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703226747);
        var items = new List<OccupancySummary>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(OccupancySummary.NewOccupancySummary($"occupancy-summary-{index}", "sample"));
        }
        var report = OccupancySummaryReport.BuildOccupancySummaryReport("test", items, clock);
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
        OccupancySummaryReportCountsEachRecordOnce();
    }
}
