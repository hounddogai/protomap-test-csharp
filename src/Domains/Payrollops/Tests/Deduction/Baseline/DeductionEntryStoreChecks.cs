namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionEntryStoreChecks
{
    public static void MemoryDeductionEntryStoreFindsWhatItSaved()
    {
        var store = DeductionEntryStore.NewMemoryDeductionEntryStore();
        var item = DeductionEntry.NewDeductionEntry("code-439", "label-275");
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
        MemoryDeductionEntryStoreFindsWhatItSaved();
    }
}
