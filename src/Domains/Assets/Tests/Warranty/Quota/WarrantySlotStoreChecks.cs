namespace Acme.Workforce.Assets.Tests.Warranty.Quota;

using Acme.Workforce.Assets.Warranty.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class WarrantySlotStoreChecks
{
    public static void MemoryWarrantySlotStoreFindsWhatItSaved()
    {
        var store = WarrantySlotStore.NewMemoryWarrantySlotStore();
        var item = WarrantySlot.NewWarrantySlot("code-33", "label-806");
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
        MemoryWarrantySlotStoreFindsWhatItSaved();
    }
}
