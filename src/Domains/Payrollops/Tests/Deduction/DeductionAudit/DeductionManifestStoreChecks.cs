namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class DeductionManifestStoreChecks
{
    public static void MemoryDeductionManifestStoreFindsWhatItSaved()
    {
        var store = DeductionManifestStore.NewMemoryDeductionManifestStore();
        var item = DeductionManifest.NewDeductionManifest("code-561", "name-54");
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
        MemoryDeductionManifestStoreFindsWhatItSaved();
    }
}
