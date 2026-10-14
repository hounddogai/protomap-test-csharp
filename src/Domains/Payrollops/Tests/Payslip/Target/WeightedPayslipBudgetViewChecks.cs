namespace Acme.Workforce.Payrollops.Tests.Payslip.Target;

using Acme.Workforce.Payrollops.Payslip.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class WeightedPayslipBudgetViewChecks
{
    public static void WeightedPayslipBudgetViewShowsTheStatusInCapitals()
    {
        var item = WeightedPayslipBudget.NewWeightedPayslipBudget("code-645", "title-898");
        var view = WeightedPayslipBudgetView.ToWeightedPayslipBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedPayslipBudgetView.ToWeightedPayslipBudgetViews(new List<WeightedPayslipBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedPayslipBudgetViewShowsTheStatusInCapitals();
    }
}
