namespace Acme.Workforce.Assets.Tests.Warranty.Notice;

using Acme.Workforce.Assets.Warranty.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class CappedWarrantyBaselineStoreChecks
{
    public static void MemoryCappedWarrantyBaselineStoreFindsWhatItSaved()
    {
        var store = CappedWarrantyBaselineStore.NewMemoryCappedWarrantyBaselineStore();
        var item = CappedWarrantyBaseline.NewCappedWarrantyBaseline("code-643", "label-816");
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
        MemoryCappedWarrantyBaselineStoreFindsWhatItSaved();
    }
}
