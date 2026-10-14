namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DeductionMappingStoreChecks
{
    public static void MemoryDeductionMappingStoreFindsWhatItSaved()
    {
        var store = DeductionMappingStore.NewMemoryDeductionMappingStore();
        var item = DeductionMapping.NewDeductionMapping("code-289", "label-7");
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
        MemoryDeductionMappingStoreFindsWhatItSaved();
    }
}
