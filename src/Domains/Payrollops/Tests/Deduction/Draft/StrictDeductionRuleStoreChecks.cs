namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictDeductionRuleStoreChecks
{
    public static void MemoryStrictDeductionRuleStoreFindsWhatItSaved()
    {
        var store = StrictDeductionRuleStore.NewMemoryStrictDeductionRuleStore();
        var item = StrictDeductionRule.NewStrictDeductionRule("code-674", "label-252");
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
        MemoryStrictDeductionRuleStoreFindsWhatItSaved();
    }
}
