namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class CappedPayslipRuleStoreChecks
{
    public static void MemoryCappedPayslipRuleStoreFindsWhatItSaved()
    {
        var store = CappedPayslipRuleStore.NewMemoryCappedPayslipRuleStore();
        var item = CappedPayslipRule.NewCappedPayslipRule("code-44", "title-484");
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
        MemoryCappedPayslipRuleStoreFindsWhatItSaved();
    }
}
