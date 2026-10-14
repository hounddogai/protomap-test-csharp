namespace Acme.Workforce.Contractors.Tests.Vendor.Score;

using Acme.Workforce.Contractors.Vendor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class VendorScoreStoreChecks
{
    public static void MemoryVendorScoreStoreFindsWhatItSaved()
    {
        var store = VendorScoreStore.NewMemoryVendorScoreStore();
        var item = VendorScore.NewVendorScore("code-745", "label-433");
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
        MemoryVendorScoreStoreFindsWhatItSaved();
    }
}
