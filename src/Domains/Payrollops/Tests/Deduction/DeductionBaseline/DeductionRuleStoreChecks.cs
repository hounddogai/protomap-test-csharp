namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBaseline;

using Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

/// <summary>Checks of deduction baseline code, run by the project's runner.</summary>
public static class DeductionRuleStoreChecks
{
    public static void MemoryDeductionRuleStoreFindsWhatItSaved()
    {
        var store = DeductionRuleStore.NewMemoryDeductionRuleStore();
        var item = DeductionRule.NewDeductionRule("code-436", "name-264");
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
        MemoryDeductionRuleStoreFindsWhatItSaved();
    }
}
