namespace Acme.Workforce.Orgdesign.Tests.Span.Ledger;

using Acme.Workforce.Orgdesign.Span.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class SpanTierStoreChecks
{
    public static void MemorySpanTierStoreFindsWhatItSaved()
    {
        var store = SpanTierStore.NewMemorySpanTierStore();
        var item = SpanTier.NewSpanTier("code-792", "name-347");
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
        MemorySpanTierStoreFindsWhatItSaved();
    }
}
