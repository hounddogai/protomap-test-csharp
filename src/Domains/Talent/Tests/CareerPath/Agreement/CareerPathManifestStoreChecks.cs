namespace Acme.Workforce.Talent.Tests.CareerPath.Agreement;

using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CareerPathManifestStoreChecks
{
    public static void MemoryCareerPathManifestStoreFindsWhatItSaved()
    {
        var store = CareerPathManifestStore.NewMemoryCareerPathManifestStore();
        var item = CareerPathManifest.NewCareerPathManifest("code-597", "title-514");
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
        MemoryCareerPathManifestStoreFindsWhatItSaved();
    }
}
