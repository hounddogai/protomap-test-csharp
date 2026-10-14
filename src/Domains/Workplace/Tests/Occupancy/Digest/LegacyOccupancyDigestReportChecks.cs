namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LegacyOccupancyDigestReportChecks
{
    public static void LegacyOccupancyDigestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708903047);
        var items = new List<LegacyOccupancyDigest>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(LegacyOccupancyDigest.NewLegacyOccupancyDigest($"legacy-occupancy-digest-{index}", "sample"));
        }
        var report = LegacyOccupancyDigestReport.BuildLegacyOccupancyDigestReport("test", items, clock);
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
        LegacyOccupancyDigestReportCountsEachRecordOnce();
    }
}
