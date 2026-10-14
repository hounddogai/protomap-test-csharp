namespace Acme.Workforce.Payrollops.Tests.PayRun.Threshold;

using Acme.Workforce.Payrollops.PayRun.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayRunBudgetViewChecks
{
    public static void PayRunBudgetViewShowsTheStatusInCapitals()
    {
        var item = PayRunBudget.NewPayRunBudget("code-742", "name-701");
        var view = PayRunBudgetView.ToPayRunBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayRunBudgetView.ToPayRunBudgetViews(new List<PayRunBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayRunBudgetViewShowsTheStatusInCapitals();
    }
}
