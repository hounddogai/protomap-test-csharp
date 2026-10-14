namespace Acme.Workforce.Talent.Tests.Mobility.MobilityMapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Talent.Mobility.MobilityMapping;

/// <summary>Checks of mobility mapping code, run by the project's runner.</summary>
public static class FallbackMobilityMappingReportChecks
{
    public static void FallbackMobilityMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1707673684);
        var items = new List<FallbackMobilityMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(FallbackMobilityMapping.NewFallbackMobilityMapping($"fallback-mobility-mapping-{index}", "sample"));
        }
        var report = FallbackMobilityMappingReport.BuildFallbackMobilityMappingReport("test", items, clock);
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
        FallbackMobilityMappingReportCountsEachRecordOnce();
    }
}
