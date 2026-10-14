namespace Acme.Workforce.Assets.Tests.Warranty.Metric;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class WarrantyClaimStoreChecks
{
    public static void MemoryWarrantyClaimStoreFindsWhatItSaved()
    {
        var store = WarrantyClaimStore.NewMemoryWarrantyClaimStore();
        var item = WarrantyClaim.NewWarrantyClaim("code-400", "name-178");
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
        MemoryWarrantyClaimStoreFindsWhatItSaved();
    }
}
