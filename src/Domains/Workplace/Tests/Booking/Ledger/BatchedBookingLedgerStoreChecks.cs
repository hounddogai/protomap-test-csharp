namespace Acme.Workforce.Workplace.Tests.Booking.Ledger;

using Acme.Workforce.Workplace.Booking.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedBookingLedgerStoreChecks
{
    public static void MemoryBatchedBookingLedgerStoreFindsWhatItSaved()
    {
        var store = BatchedBookingLedgerStore.NewMemoryBatchedBookingLedgerStore();
        var item = BatchedBookingLedger.NewBatchedBookingLedger("code-203", "label-150");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryBatchedBookingLedgerStoreFindsWhatItSaved();
    }
}
