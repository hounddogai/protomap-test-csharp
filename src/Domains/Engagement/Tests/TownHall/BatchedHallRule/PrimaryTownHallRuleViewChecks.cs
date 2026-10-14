namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class PrimaryTownHallRuleViewChecks
{
    public static void PrimaryTownHallRuleViewShowsTheStatusInCapitals()
    {
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("code-94", "title-996");
        var view = PrimaryTownHallRuleView.ToPrimaryTownHallRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryTownHallRuleView.ToPrimaryTownHallRuleViews(new List<PrimaryTownHallRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryTownHallRuleViewShowsTheStatusInCapitals();
    }
}
