namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanClaimStoreChecks
{
    public static void MemorySpanClaimStoreFindsWhatItSaved()
    {
        var store = SpanClaimStore.NewMemorySpanClaimStore();
        var item = SpanClaim.NewSpanClaim("code-490", "name-450");
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
        MemorySpanClaimStoreFindsWhatItSaved();
    }
}
