namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class WarrantyAdjustmentStoreChecks
{
    public static void MemoryWarrantyAdjustmentStoreFindsWhatItSaved()
    {
        var store = WarrantyAdjustmentStore.NewMemoryWarrantyAdjustmentStore();
        var item = WarrantyAdjustment.NewWarrantyAdjustment("code-850", "title-524");
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
        MemoryWarrantyAdjustmentStoreFindsWhatItSaved();
    }
}
