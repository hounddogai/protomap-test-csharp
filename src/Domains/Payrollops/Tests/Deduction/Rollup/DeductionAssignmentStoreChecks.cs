namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionAssignmentStoreChecks
{
    public static void MemoryDeductionAssignmentStoreFindsWhatItSaved()
    {
        var store = DeductionAssignmentStore.NewMemoryDeductionAssignmentStore();
        var item = DeductionAssignment.NewDeductionAssignment("code-887", "title-87");
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
        MemoryDeductionAssignmentStoreFindsWhatItSaved();
    }
}
