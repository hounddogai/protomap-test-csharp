namespace Acme.Workforce.Workplace.Tests.Booking.Ledger;

using Acme.Workforce.Workplace.Booking.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedBookingLedgerViewChecks
{
    public static void BatchedBookingLedgerViewShowsTheStatusInCapitals()
    {
        var item = BatchedBookingLedger.NewBatchedBookingLedger("code-203", "label-150");
        var view = BatchedBookingLedgerView.ToBatchedBookingLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedBookingLedgerView.ToBatchedBookingLedgerViews(new List<BatchedBookingLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedBookingLedgerViewShowsTheStatusInCapitals();
    }
}
