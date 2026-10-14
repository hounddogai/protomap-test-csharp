namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TownHallRuleViewChecks
{
    public static void TownHallRuleViewShowsTheStatusInCapitals()
    {
        var item = TownHallRule.NewTownHallRule("code-75", "name-47");
        var view = TownHallRuleView.ToTownHallRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallRuleView.ToTownHallRuleViews(new List<TownHallRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallRuleViewShowsTheStatusInCapitals();
    }
}
