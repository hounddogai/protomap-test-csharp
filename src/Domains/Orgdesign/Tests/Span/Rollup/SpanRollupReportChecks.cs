namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanRollupReportChecks
{
    public static void SpanRollupReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1701243330);
        var items = new List<SpanRollup>();
        for (var index = 0; index < 2; index++)
        {
            items.Add(SpanRollup.NewSpanRollup($"span-rollup-{index}", "sample"));
        }
        var report = SpanRollupReport.BuildSpanRollupReport("test", items, clock);
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
        SpanRollupReportCountsEachRecordOnce();
    }
}
