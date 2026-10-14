namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanEstimateReportChecks
{
    public static void SpanEstimateReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700134235);
        var items = new List<SpanEstimate>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SpanEstimate.NewSpanEstimate($"span-estimate-{index}", "sample"));
        }
        var report = SpanEstimateReport.BuildSpanEstimateReport("test", items, clock);
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
        SpanEstimateReportCountsEachRecordOnce();
    }
}
