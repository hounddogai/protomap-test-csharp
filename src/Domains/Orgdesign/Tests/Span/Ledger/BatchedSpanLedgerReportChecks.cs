namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedSpanLedgerReportChecks
{
    public static void BatchedSpanLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1700779702);
        var items = new List<BatchedSpanLedger>();
        for (var index = 0; index < 6; index++)
        {
            items.Add(BatchedSpanLedger.NewBatchedSpanLedger($"batched-span-ledger-{index}", "sample"));
        }
        var report = BatchedSpanLedgerReport.BuildBatchedSpanLedgerReport("test", items, clock);
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
        BatchedSpanLedgerReportCountsEachRecordOnce();
    }
}
