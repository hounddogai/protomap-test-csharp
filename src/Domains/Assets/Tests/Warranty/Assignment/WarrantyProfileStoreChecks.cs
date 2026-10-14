namespace Acme.Workforce.Assets.Tests.Warranty.Assignment;

using Acme.Workforce.Assets.Warranty.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class WarrantyProfileStoreChecks
{
    public static void MemoryWarrantyProfileStoreFindsWhatItSaved()
    {
        var store = WarrantyProfileStore.NewMemoryWarrantyProfileStore();
        var item = WarrantyProfile.NewWarrantyProfile("code-139", "title-247");
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
        MemoryWarrantyProfileStoreFindsWhatItSaved();
    }
}
