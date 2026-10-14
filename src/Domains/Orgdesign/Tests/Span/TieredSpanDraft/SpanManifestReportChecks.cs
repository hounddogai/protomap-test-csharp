namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanManifestReportChecks
{
    public static void SpanManifestReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706003697);
        var items = new List<SpanManifest>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SpanManifest.NewSpanManifest($"span-manifest-{index}", "sample"));
        }
        var report = SpanManifestReport.BuildSpanManifestReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 2, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SpanManifestReportCountsEachRecordOnce();
    }
}
