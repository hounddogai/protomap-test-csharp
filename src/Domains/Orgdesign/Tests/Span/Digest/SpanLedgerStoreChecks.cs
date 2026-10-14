namespace Acme.Workforce.Orgdesign.Tests.Span.Digest;

using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SpanLedgerStoreChecks
{
    public static void MemorySpanLedgerStoreFindsWhatItSaved()
    {
        var store = SpanLedgerStore.NewMemorySpanLedgerStore();
        var item = SpanLedger.NewSpanLedger("code-459", "name-31");
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
        MemorySpanLedgerStoreFindsWhatItSaved();
    }
}
