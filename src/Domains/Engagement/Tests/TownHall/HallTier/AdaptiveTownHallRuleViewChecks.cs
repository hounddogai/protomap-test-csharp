namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class AdaptiveTownHallRuleViewChecks
{
    public static void AdaptiveTownHallRuleViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("code-965", "name-525");
        var view = AdaptiveTownHallRuleView.ToAdaptiveTownHallRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveTownHallRuleView.ToAdaptiveTownHallRuleViews(new List<AdaptiveTownHallRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveTownHallRuleViewShowsTheStatusInCapitals();
    }
}
