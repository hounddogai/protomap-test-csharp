namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledMobilityManifestStoreChecks
{
    public static void MemoryScheduledMobilityManifestStoreFindsWhatItSaved()
    {
        var store = ScheduledMobilityManifestStore.NewMemoryScheduledMobilityManifestStore();
        var item = ScheduledMobilityManifest.NewScheduledMobilityManifest("code-764", "label-798");
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
        MemoryScheduledMobilityManifestStoreFindsWhatItSaved();
    }
}
