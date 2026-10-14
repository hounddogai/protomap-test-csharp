namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class RegionalSpanDigestStoreChecks
{
    public static void MemoryRegionalSpanDigestStoreFindsWhatItSaved()
    {
        var store = RegionalSpanDigestStore.NewMemoryRegionalSpanDigestStore();
        var item = RegionalSpanDigest.NewRegionalSpanDigest("code-45", "label-478");
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
        MemoryRegionalSpanDigestStoreFindsWhatItSaved();
    }
}
