namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RollingPositionManifestStoreChecks
{
    public static void MemoryRollingPositionManifestStoreFindsWhatItSaved()
    {
        var store = RollingPositionManifestStore.NewMemoryRollingPositionManifestStore();
        var item = RollingPositionManifest.NewRollingPositionManifest("code-469", "name-133");
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
        MemoryRollingPositionManifestStoreFindsWhatItSaved();
    }
}
