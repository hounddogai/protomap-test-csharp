namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class DefaultAccessBadgeManifestStoreChecks
{
    public static void MemoryDefaultAccessBadgeManifestStoreFindsWhatItSaved()
    {
        var store = DefaultAccessBadgeManifestStore.NewMemoryDefaultAccessBadgeManifestStore();
        var item = DefaultAccessBadgeManifest.NewDefaultAccessBadgeManifest("code-720", "label-694");
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
        MemoryDefaultAccessBadgeManifestStoreFindsWhatItSaved();
    }
}
