namespace Acme.Workforce.Contractors.Tests.Engagement.Manifest;

using Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class LenientEngagementManifestStoreChecks
{
    public static void MemoryLenientEngagementManifestStoreFindsWhatItSaved()
    {
        var store = LenientEngagementManifestStore.NewMemoryLenientEngagementManifestStore();
        var item = LenientEngagementManifest.NewLenientEngagementManifest("code-891", "label-522");
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
        MemoryLenientEngagementManifestStoreFindsWhatItSaved();
    }
}
