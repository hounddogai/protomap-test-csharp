namespace Acme.Workforce.Orgdesign.Tests.Span.Digest;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SpanLedgerReportChecks
{
    public static void SpanLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1706260299);
        var items = new List<SpanLedger>();
        for (var index = 0; index < 4; index++)
        {
            items.Add(SpanLedger.NewSpanLedger($"span-ledger-{index}", "sample"));
        }
        var report = SpanLedgerReport.BuildSpanLedgerReport("test", items, clock);
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
        SpanLedgerReportCountsEachRecordOnce();
    }
}
