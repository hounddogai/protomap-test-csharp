namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TownHallRuleChecks
{
    public static void TownHallRuleStartsInNew()
    {
        var item = TownHallRule.NewTownHallRule("code-75", "name-47");
        Check.Equal(item.Status, TownHallRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallRuleMovesToQueued()
    {
        var item = TownHallRule.NewTownHallRule("code-75", "name-47");
        try
        {
            item.MoveTo(TownHallRuleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallRuleStatus.Queued, "status");
    }

    public static void TownHallRuleRejectsMovesFromFailed()
    {
        var item = TownHallRule.NewTownHallRule("code-75", "name-47");
        item.Status = TownHallRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(TownHallRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        TownHallRuleStartsInNew();
        TownHallRuleMovesToQueued();
        TownHallRuleRejectsMovesFromFailed();
    }
}
