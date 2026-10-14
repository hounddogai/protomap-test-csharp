namespace Acme.Workforce.Orgdesign.Tests.Span.SpanDraft;

using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Checks of span draft code, run by the project's runner.</summary>
public static class DefaultSpanClaimStoreChecks
{
    public static void MemoryDefaultSpanClaimStoreFindsWhatItSaved()
    {
        var store = DefaultSpanClaimStore.NewMemoryDefaultSpanClaimStore();
        var item = DefaultSpanClaim.NewDefaultSpanClaim("code-603", "label-489");
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
        MemoryDefaultSpanClaimStoreFindsWhatItSaved();
    }
}
