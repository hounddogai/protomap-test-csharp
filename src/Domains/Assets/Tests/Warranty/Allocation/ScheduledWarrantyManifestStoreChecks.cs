namespace Acme.Workforce.Assets.Tests.Warranty.Allocation;

using Acme.Workforce.Assets.Warranty.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ScheduledWarrantyManifestStoreChecks
{
    public static void MemoryScheduledWarrantyManifestStoreFindsWhatItSaved()
    {
        var store = ScheduledWarrantyManifestStore.NewMemoryScheduledWarrantyManifestStore();
        var item = ScheduledWarrantyManifest.NewScheduledWarrantyManifest("code-999", "label-483");
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
        MemoryScheduledWarrantyManifestStoreFindsWhatItSaved();
    }
}
