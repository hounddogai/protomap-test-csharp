namespace Acme.Workforce.Talent.Tests.CareerPath.Quota;

using Acme.Workforce.Talent.CareerPath.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class FallbackCareerPathManifestStoreChecks
{
    public static void MemoryFallbackCareerPathManifestStoreFindsWhatItSaved()
    {
        var store = FallbackCareerPathManifestStore.NewMemoryFallbackCareerPathManifestStore();
        var item = FallbackCareerPathManifest.NewFallbackCareerPathManifest("code-978", "title-629");
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
        MemoryFallbackCareerPathManifestStoreFindsWhatItSaved();
    }
}
