namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SpanTierReportChecks
{
    public static void SpanTierReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1703819976);
        var items = new List<SpanTier>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(SpanTier.NewSpanTier($"span-tier-{index}", "sample"));
        }
        var report = SpanTierReport.BuildSpanTierReport("test", items, clock);
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
        SpanTierReportCountsEachRecordOnce();
    }
}
