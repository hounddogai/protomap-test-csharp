namespace Acme.Workforce.Payrollops.Tests.Deduction.Metric;

using Acme.Workforce.Payrollops.Deduction.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedDeductionPolicyStoreChecks
{
    public static void MemoryCappedDeductionPolicyStoreFindsWhatItSaved()
    {
        var store = CappedDeductionPolicyStore.NewMemoryCappedDeductionPolicyStore();
        var item = CappedDeductionPolicy.NewCappedDeductionPolicy("code-743", "name-96");
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
        MemoryCappedDeductionPolicyStoreFindsWhatItSaved();
    }
}
