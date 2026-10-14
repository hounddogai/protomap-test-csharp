namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanOverrideReportChecks
{
    public static void SpanOverrideReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1708407013);
        var items = new List<SpanOverride>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(SpanOverride.NewSpanOverride($"span-override-{index}", "sample"));
        }
        var report = SpanOverrideReport.BuildSpanOverrideReport("test", items, clock);
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
        SpanOverrideReportCountsEachRecordOnce();
    }
}
