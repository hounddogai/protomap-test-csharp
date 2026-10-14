namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class StrictReorgBudgetViewChecks
{
    public static void StrictReorgBudgetViewShowsTheStatusInCapitals()
    {
        var item = StrictReorgBudget.NewStrictReorgBudget("code-640", "name-345");
        var view = StrictReorgBudgetView.ToStrictReorgBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictReorgBudgetView.ToStrictReorgBudgetViews(new List<StrictReorgBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictReorgBudgetViewShowsTheStatusInCapitals();
    }
}
