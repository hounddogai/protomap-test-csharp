namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanBatchReportChecks
{
    public static void SpanBatchReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700754028);
        var items = new List<SpanBatch>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SpanBatch.NewSpanBatch($"span-batch-{index}", "sample"));
        }
        var report = SpanBatchReport.BuildSpanBatchReport("test", items, clock);
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
        SpanBatchReportCountsEachRecordOnce();
    }
}
