namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Item;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanRuleViewChecks
{
    public static void SeasonalSuccessionPlanRuleViewShowsTheStatusInCapitals()
    {
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("code-48", "title-21");
        var view = SeasonalSuccessionPlanRuleView.ToSeasonalSuccessionPlanRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalSuccessionPlanRuleView.ToSeasonalSuccessionPlanRuleViews(new List<SeasonalSuccessionPlanRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalSuccessionPlanRuleViewShowsTheStatusInCapitals();
    }
}
