namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolBudgetViewChecks
{
    public static void PoolBudgetViewShowsTheStatusInCapitals()
    {
        var item = PoolBudget.NewPoolBudget("code-372", "name-376");
        var view = PoolBudgetView.ToPoolBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolBudgetView.ToPoolBudgetViews(new List<PoolBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolBudgetViewShowsTheStatusInCapitals();
    }
}
