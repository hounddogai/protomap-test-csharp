namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedSpanLedgerStoreChecks
{
    public static void MemoryBatchedSpanLedgerStoreFindsWhatItSaved()
    {
        var store = BatchedSpanLedgerStore.NewMemoryBatchedSpanLedgerStore();
        var item = BatchedSpanLedger.NewBatchedSpanLedger("code-286", "title-64");
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
        MemoryBatchedSpanLedgerStoreFindsWhatItSaved();
    }
}
