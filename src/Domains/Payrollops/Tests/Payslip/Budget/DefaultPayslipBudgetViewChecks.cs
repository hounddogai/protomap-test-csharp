namespace Acme.Workforce.Payrollops.Tests.Payslip.Budget;

using Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DefaultPayslipBudgetViewChecks
{
    public static void DefaultPayslipBudgetViewShowsTheStatusInCapitals()
    {
        var item = DefaultPayslipBudget.NewDefaultPayslipBudget("code-673", "title-470");
        var view = DefaultPayslipBudgetView.ToDefaultPayslipBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPayslipBudgetView.ToDefaultPayslipBudgetViews(new List<DefaultPayslipBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPayslipBudgetViewShowsTheStatusInCapitals();
    }
}
