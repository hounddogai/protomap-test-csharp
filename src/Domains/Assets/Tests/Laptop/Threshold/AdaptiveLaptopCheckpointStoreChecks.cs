namespace Acme.Workforce.Assets.Tests.Laptop.Threshold;

using Acme.Workforce.Assets.Laptop.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveLaptopCheckpointStoreChecks
{
    public static void MemoryAdaptiveLaptopCheckpointStoreFindsWhatItSaved()
    {
        var store = AdaptiveLaptopCheckpointStore.NewMemoryAdaptiveLaptopCheckpointStore();
        var item = AdaptiveLaptopCheckpoint.NewAdaptiveLaptopCheckpoint("code-968", "label-883");
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
        MemoryAdaptiveLaptopCheckpointStoreFindsWhatItSaved();
    }
}
