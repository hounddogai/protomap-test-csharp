namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalDeductionBatchStoreChecks
{
    public static void MemoryRegionalDeductionBatchStoreFindsWhatItSaved()
    {
        var store = RegionalDeductionBatchStore.NewMemoryRegionalDeductionBatchStore();
        var item = RegionalDeductionBatch.NewRegionalDeductionBatch("code-563", "name-304");
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
        MemoryRegionalDeductionBatchStoreFindsWhatItSaved();
    }
}
