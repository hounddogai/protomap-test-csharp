namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantBudgetViewChecks
{
    public static void EquityGrantBudgetViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantBudget.NewEquityGrantBudget("code-369", "label-18");
        var view = EquityGrantBudgetView.ToEquityGrantBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantBudgetView.ToEquityGrantBudgetViews(new List<EquityGrantBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantBudgetViewShowsTheStatusInCapitals();
    }
}
