namespace Acme.Workforce.Contractors.Tests.Vendor.Proposal;

using Acme.Workforce.Contractors.Vendor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultVendorScoreStoreChecks
{
    public static void MemoryDefaultVendorScoreStoreFindsWhatItSaved()
    {
        var store = DefaultVendorScoreStore.NewMemoryDefaultVendorScoreStore();
        var item = DefaultVendorScore.NewDefaultVendorScore("code-630", "name-593");
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
        MemoryDefaultVendorScoreStoreFindsWhatItSaved();
    }
}
