namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CappedDeductionAssignmentStoreChecks
{
    public static void MemoryCappedDeductionAssignmentStoreFindsWhatItSaved()
    {
        var store = CappedDeductionAssignmentStore.NewMemoryCappedDeductionAssignmentStore();
        var item = CappedDeductionAssignment.NewCappedDeductionAssignment("code-587", "label-243");
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
        MemoryCappedDeductionAssignmentStoreFindsWhatItSaved();
    }
}
