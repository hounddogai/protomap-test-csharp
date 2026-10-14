namespace Acme.Workforce.Orgdesign.Tests.Span.Digest;

using Acme.Workforce.Orgdesign.Span.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SpanDigestStoreChecks
{
    public static void MemorySpanDigestStoreFindsWhatItSaved()
    {
        var store = SpanDigestStore.NewMemorySpanDigestStore();
        var item = SpanDigest.NewSpanDigest("code-675", "name-571");
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
        MemorySpanDigestStoreFindsWhatItSaved();
    }
}
