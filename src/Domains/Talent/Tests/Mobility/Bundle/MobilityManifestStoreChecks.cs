namespace Acme.Workforce.Talent.Tests.Mobility.Bundle;

using Acme.Workforce.Talent.Mobility.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class MobilityManifestStoreChecks
{
    public static void MemoryMobilityManifestStoreFindsWhatItSaved()
    {
        var store = MobilityManifestStore.NewMemoryMobilityManifestStore();
        var item = MobilityManifest.NewMobilityManifest("code-55", "name-247");
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
        MemoryMobilityManifestStoreFindsWhatItSaved();
    }
}
