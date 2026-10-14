namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopCheckpointStoreChecks
{
    public static void MemoryLaptopCheckpointStoreFindsWhatItSaved()
    {
        var store = LaptopCheckpointStore.NewMemoryLaptopCheckpointStore();
        var item = LaptopCheckpoint.NewLaptopCheckpoint("code-641", "label-4");
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
        MemoryLaptopCheckpointStoreFindsWhatItSaved();
    }
}
