namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateManifestStoreChecks
{
    public static void MemoryRateManifestStoreFindsWhatItSaved()
    {
        var store = RateManifestStore.NewMemoryRateManifestStore();
        var item = RateManifest.NewRateManifest("code-863", "label-817");
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
        MemoryRateManifestStoreFindsWhatItSaved();
    }
}
