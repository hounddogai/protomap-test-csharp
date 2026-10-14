namespace Acme.Workforce.Assets.Tests.Warranty.Adjustment;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyWarrantyBaselineStoreChecks
{
    public static void MemoryLegacyWarrantyBaselineStoreFindsWhatItSaved()
    {
        var store = LegacyWarrantyBaselineStore.NewMemoryLegacyWarrantyBaselineStore();
        var item = LegacyWarrantyBaseline.NewLegacyWarrantyBaseline("code-627", "label-164");
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
        MemoryLegacyWarrantyBaselineStoreFindsWhatItSaved();
    }
}
