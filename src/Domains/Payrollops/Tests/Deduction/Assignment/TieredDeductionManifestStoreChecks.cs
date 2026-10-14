namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredDeductionManifestStoreChecks
{
    public static void MemoryTieredDeductionManifestStoreFindsWhatItSaved()
    {
        var store = TieredDeductionManifestStore.NewMemoryTieredDeductionManifestStore();
        var item = TieredDeductionManifest.NewTieredDeductionManifest("code-614", "label-816");
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
        MemoryTieredDeductionManifestStoreFindsWhatItSaved();
    }
}
