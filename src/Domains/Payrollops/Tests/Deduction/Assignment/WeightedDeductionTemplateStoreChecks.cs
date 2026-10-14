namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class WeightedDeductionTemplateStoreChecks
{
    public static void MemoryWeightedDeductionTemplateStoreFindsWhatItSaved()
    {
        var store = WeightedDeductionTemplateStore.NewMemoryWeightedDeductionTemplateStore();
        var item = WeightedDeductionTemplate.NewWeightedDeductionTemplate("code-851", "label-271");
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
        MemoryWeightedDeductionTemplateStoreFindsWhatItSaved();
    }
}
