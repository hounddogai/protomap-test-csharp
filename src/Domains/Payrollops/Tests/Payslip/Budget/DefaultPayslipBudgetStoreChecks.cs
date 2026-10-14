namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DefaultPayslipBudgetStoreChecks
{
    public static void MemoryDefaultPayslipBudgetStoreFindsWhatItSaved()
    {
        var store = DefaultPayslipBudgetStore.NewMemoryDefaultPayslipBudgetStore();
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("code-673", "title-470");
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
        MemoryDefaultPayslipBudgetStoreFindsWhatItSaved();
    }
}
