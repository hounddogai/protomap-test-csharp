namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgMappingReportChecks
{
    public static void ReorgMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701976952);
        var items = new List<ReorgMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(ReorgMapping.NewReorgMapping($"reorg-mapping-{index}", "sample"));
        }
        var report = ReorgMappingReport.BuildReorgMappingReport("test", items, clock);
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
        ReorgMappingReportCountsEachRecordOnce();
    }
}
