namespace Acme.Workforce.Workplace.Tests.Booking.Ledger;

using Acme.Workforce.Kit.Testing;
using Acme.Workforce.Workplace.Booking.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedBookingLedgerReportChecks
{
    public static void BatchedBookingLedgerReportCountsEachRecordOnce()
    {
        var clock = new FixedClock(1705241162);
        var items = new List<BatchedBookingLedger>();
        for (var index = 0; index < 3; index++)
        {
            items.Add(BatchedBookingLedger.NewBatchedBookingLedger($"batched-booking-ledger-{index}", "sample"));
        }
        var report = BatchedBookingLedgerReport.BuildBatchedBookingLedgerReport("test", items, clock);
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
        BatchedBookingLedgerReportCountsEachRecordOnce();
    }
}
