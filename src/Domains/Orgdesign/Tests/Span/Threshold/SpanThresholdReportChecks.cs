namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class SpanThresholdReportChecks
{
    public static void SpanThresholdReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701540115);
        var items = new List<SpanThreshold>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SpanThreshold.NewSpanThreshold($"span-threshold-{index}", "sample"));
        }
        var report = SpanThresholdReport.BuildSpanThresholdReport("test", items, clock);
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
        SpanThresholdReportCountsEachRecordOnce();
    }
}
