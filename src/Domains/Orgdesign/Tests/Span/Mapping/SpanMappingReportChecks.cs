namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class SpanMappingReportChecks
{
    public static void SpanMappingReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702045215);
        var items = new List<SpanMapping>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SpanMapping.NewSpanMapping($"span-mapping-{index}", "sample"));
        }
        var report = SpanMappingReport.BuildSpanMappingReport("test", items, clock);
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
        SpanMappingReportCountsEachRecordOnce();
    }
}
