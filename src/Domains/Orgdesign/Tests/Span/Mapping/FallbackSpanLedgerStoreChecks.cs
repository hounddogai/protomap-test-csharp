namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class FallbackSpanLedgerStoreChecks
{
    public static void MemoryFallbackSpanLedgerStoreFindsWhatItSaved()
    {
        var store = FallbackSpanLedgerStore.NewMemoryFallbackSpanLedgerStore();
        var item = FallbackSpanLedger.NewFallbackSpanLedger("code-268", "name-932");
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
        MemoryFallbackSpanLedgerStoreFindsWhatItSaved();
    }
}
