namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeductionMilestoneStoreChecks
{
    public static void MemoryDeductionMilestoneStoreFindsWhatItSaved()
    {
        var store = DeductionMilestoneStore.NewMemoryDeductionMilestoneStore();
        var item = DeductionMilestone.NewDeductionMilestone("code-928", "name-634");
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
        MemoryDeductionMilestoneStoreFindsWhatItSaved();
    }
}
