namespace Acme.Workforce.Workplace.Tests.Booking.Ledger;

using Acme.Workforce.Workplace.Booking.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedBookingLedgerChecks
{
    public static void BatchedBookingLedgerStartsInPlanned()
    {
        var item = BatchedBookingLedger.NewBatchedBookingLedger("code-203", "label-150");
        Check.Equal(item.Status, BatchedBookingLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedBookingLedgerMovesToInProgress()
    {
        var item = BatchedBookingLedger.NewBatchedBookingLedger("code-203", "label-150");
        try
        {
            item.MoveTo(BatchedBookingLedgerStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedBookingLedgerStatus.InProgress, "status");
    }

    public static void BatchedBookingLedgerRejectsMovesFromCancelled()
    {
        var item = BatchedBookingLedger.NewBatchedBookingLedger("code-203", "label-150");
        item.Status = BatchedBookingLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedBookingLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedBookingLedgerStartsInPlanned();
        BatchedBookingLedgerMovesToInProgress();
        BatchedBookingLedgerRejectsMovesFromCancelled();
    }
}
