namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class AdaptiveReorgBudgetViewChecks
{
    public static void AdaptiveReorgBudgetViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveReorgBudget.NewAdaptiveReorgBudget("code-881", "title-898");
        var view = AdaptiveReorgBudgetView.ToAdaptiveReorgBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveReorgBudgetView.ToAdaptiveReorgBudgetViews(new List<AdaptiveReorgBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveReorgBudgetViewShowsTheStatusInCapitals();
    }
}
