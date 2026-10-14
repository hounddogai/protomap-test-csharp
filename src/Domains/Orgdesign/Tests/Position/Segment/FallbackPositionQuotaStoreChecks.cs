namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class FallbackPositionQuotaStoreChecks
{
    public static void MemoryFallbackPositionQuotaStoreFindsWhatItSaved()
    {
        var store = FallbackPositionQuotaStore.NewMemoryFallbackPositionQuotaStore();
        var item = FallbackPositionQuota.NewFallbackPositionQuota("code-980", "title-730");
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
        MemoryFallbackPositionQuotaStoreFindsWhatItSaved();
    }
}
