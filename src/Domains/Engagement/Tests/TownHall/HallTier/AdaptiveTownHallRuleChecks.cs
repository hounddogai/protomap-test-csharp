namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class AdaptiveTownHallRuleChecks
{
    public static void AdaptiveTownHallRuleStartsInOpen()
    {
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("code-965", "name-525");
        Check.Equal(item.Status, AdaptiveTownHallRuleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTownHallRuleMovesToInReview()
    {
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("code-965", "name-525");
        try
        {
            item.MoveTo(AdaptiveTownHallRuleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTownHallRuleStatus.InReview, "status");
    }

    public static void AdaptiveTownHallRuleRejectsMovesFromArchived()
    {
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("code-965", "name-525");
        item.Status = AdaptiveTownHallRuleStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveTownHallRuleStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveTownHallRuleStartsInOpen();
        AdaptiveTownHallRuleMovesToInReview();
        AdaptiveTownHallRuleRejectsMovesFromArchived();
    }
}
