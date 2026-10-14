namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DeductionGroupStoreChecks
{
    public static void MemoryDeductionGroupStoreFindsWhatItSaved()
    {
        var store = DeductionGroupStore.NewMemoryDeductionGroupStore();
        var item = DeductionGroup.NewDeductionGroup("code-742", "title-302");
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
        MemoryDeductionGroupStoreFindsWhatItSaved();
    }
}
