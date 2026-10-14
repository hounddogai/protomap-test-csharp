namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class CappedTaxFormRuleStoreChecks
{
    public static void MemoryCappedTaxFormRuleStoreFindsWhatItSaved()
    {
        var store = CappedTaxFormRuleStore.NewMemoryCappedTaxFormRuleStore();
        var item = CappedTaxFormRule.NewCappedTaxFormRule("code-573", "name-111");
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
        MemoryCappedTaxFormRuleStoreFindsWhatItSaved();
    }
}
