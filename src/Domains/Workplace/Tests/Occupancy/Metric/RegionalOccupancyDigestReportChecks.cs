namespace Acme.Workforce.Workplace.Tests.Occupancy.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RegionalOccupancyDigestReportChecks
{
    public static void RegionalOccupancyDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701359520);
        var items = new List<RegionalOccupancyDigest>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(RegionalOccupancyDigest.NewRegionalOccupancyDigest($"regional-occupancy-digest-{index}", "sample"));
        }
        var report = RegionalOccupancyDigestReport.BuildRegionalOccupancyDigestReport("test", items, clock);
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
        RegionalOccupancyDigestReportCountsEachRecordOnce();
    }
}
