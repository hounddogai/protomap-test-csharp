namespace Acme.Workforce.Assets.Tests.Laptop.Milestone;

using Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LaptopSegmentStoreChecks
{
    public static void MemoryLaptopSegmentStoreFindsWhatItSaved()
    {
        var store = LaptopSegmentStore.NewMemoryLaptopSegmentStore();
        var item = LaptopSegment.NewLaptopSegment("code-342", "name-13");
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
        MemoryLaptopSegmentStoreFindsWhatItSaved();
    }
}
