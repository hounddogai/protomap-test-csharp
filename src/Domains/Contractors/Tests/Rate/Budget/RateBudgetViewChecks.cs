namespace Acme.Workforce.Contractors.Tests.Rate.Budget;

using Acme.Workforce.Contractors.Rate.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RateBudgetViewChecks
{
    public static void RateBudgetViewShowsTheStatusInCapitals()
    {
        var item = RateBudget.NewRateBudget("code-20", "name-133");
        var view = RateBudgetView.ToRateBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateBudgetView.ToRateBudgetViews(new List<RateBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateBudgetViewShowsTheStatusInCapitals();
    }
}
