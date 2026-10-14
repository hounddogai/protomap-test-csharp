namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SpanWindowReportChecks
{
    public static void SpanWindowReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1702527786);
        var items = new List<SpanWindow>();
        for (var index = 0; index < 5; index++)
        {
            items.Add(SpanWindow.NewSpanWindow($"span-window-{index}", "sample"));
        }
        var report = SpanWindowReport.BuildSpanWindowReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 5, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        SpanWindowReportCountsEachRecordOnce();
    }
}
