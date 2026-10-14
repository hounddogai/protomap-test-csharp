namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanBudgetViewChecks
{
    public static void SeasonalSuccessionPlanBudgetViewShowsTheStatusInCapitals()
    {
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("code-355", "label-953");
        var view = SeasonalSuccessionPlanBudgetView.ToSeasonalSuccessionPlanBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalSuccessionPlanBudgetView.ToSeasonalSuccessionPlanBudgetViews(new List<SeasonalSuccessionPlanBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanBudgetViewShowsTheStatusInCapitals();
    }
}
