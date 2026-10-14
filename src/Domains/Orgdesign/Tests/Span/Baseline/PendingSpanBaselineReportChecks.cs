namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingSpanBaselineReportChecks
{
    public static void PendingSpanBaselineReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706834765);
        var items = new List<PendingSpanBaseline>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(PendingSpanBaseline.NewPendingSpanBaseline($"pending-span-baseline-{index}", "sample"));
        }
        var report = PendingSpanBaselineReport.BuildPendingSpanBaselineReport("test", items, clock);
        var counted = 0;
        foreach (var key in report.Counts.Keys.Order(StringComparer.Ordinal))
        {
            var valueValue = report.Counts[key];
            counted += valueValue;
        }
        Check.True(counted <= 6, "counted");
        Check.Equal(report.GeneratedAt, clock.Now(), "generated at");
    }

    public static void RunAll()
    {
        PendingSpanBaselineReportCountsEachRecordOnce();
    }
}
