namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class WeightedDeductionPlanStoreChecks
{
    public static void MemoryWeightedDeductionPlanStoreFindsWhatItSaved()
    {
        var store = WeightedDeductionPlanStore.NewMemoryWeightedDeductionPlanStore();
        var item = WeightedDeductionPlan.NewWeightedDeductionPlan("code-461", "label-666");
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
        MemoryWeightedDeductionPlanStoreFindsWhatItSaved();
    }
}
