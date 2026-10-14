namespace Acme.Workforce.Assets.Tests.Laptop.Baseline;

using Acme.Workforce.Assets.Laptop.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LaptopSlotStoreChecks
{
    public static void MemoryLaptopSlotStoreFindsWhatItSaved()
    {
        var store = LaptopSlotStore.NewMemoryLaptopSlotStore();
        var item = LaptopSlot.NewLaptopSlot("code-230", "title-118");
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
        MemoryLaptopSlotStoreFindsWhatItSaved();
    }
}
