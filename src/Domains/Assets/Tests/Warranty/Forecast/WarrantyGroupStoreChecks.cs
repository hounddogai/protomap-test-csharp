namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyGroupStoreChecks
{
    public static void MemoryWarrantyGroupStoreFindsWhatItSaved()
    {
        var store = WarrantyGroupStore.NewMemoryWarrantyGroupStore();
        var item = WarrantyGroup.NewWarrantyGroup("code-824", "name-685");
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
        MemoryWarrantyGroupStoreFindsWhatItSaved();
    }
}
