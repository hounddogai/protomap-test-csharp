namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class WarrantyDigestStoreChecks
{
    public static void MemoryWarrantyDigestStoreFindsWhatItSaved()
    {
        var store = WarrantyDigestStore.NewMemoryWarrantyDigestStore();
        var item = WarrantyDigest.NewWarrantyDigest("code-70", "label-447");
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
        MemoryWarrantyDigestStoreFindsWhatItSaved();
    }
}
