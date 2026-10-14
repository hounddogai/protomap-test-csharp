namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBudget;

using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Checks of deduction budget code, run by the project's runner.</summary>
public static class RegionalDeductionTemplateStoreChecks
{
    public static void MemoryRegionalDeductionTemplateStoreFindsWhatItSaved()
    {
        var store = RegionalDeductionTemplateStore.NewMemoryRegionalDeductionTemplateStore();
        var item = RegionalDeductionTemplate.NewRegionalDeductionTemplate("code-423", "name-942");
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
        MemoryRegionalDeductionTemplateStoreFindsWhatItSaved();
    }
}
