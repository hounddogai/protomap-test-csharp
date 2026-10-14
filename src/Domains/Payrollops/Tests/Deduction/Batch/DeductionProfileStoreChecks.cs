namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DeductionProfileStoreChecks
{
    public static void MemoryDeductionProfileStoreFindsWhatItSaved()
    {
        var store = DeductionProfileStore.NewMemoryDeductionProfileStore();
        var item = DeductionProfile.NewDeductionProfile("code-798", "title-392");
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
        MemoryDeductionProfileStoreFindsWhatItSaved();
    }
}
