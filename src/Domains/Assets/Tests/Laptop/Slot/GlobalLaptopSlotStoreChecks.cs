namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class GlobalLaptopSlotStoreChecks
{
    public static void MemoryGlobalLaptopSlotStoreFindsWhatItSaved()
    {
        var store = GlobalLaptopSlotStore.NewMemoryGlobalLaptopSlotStore();
        var item = GlobalLaptopSlot.NewGlobalLaptopSlot("code-624", "name-563");
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
        MemoryGlobalLaptopSlotStoreFindsWhatItSaved();
    }
}
