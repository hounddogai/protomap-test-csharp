namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class PrimaryTownHallRuleChecks
{
    public static void PrimaryTownHallRuleStartsInProposed()
    {
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("code-94", "title-996");
        Check.Equal(item.Status, PrimaryTownHallRuleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryTownHallRuleMovesToApproved()
    {
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("code-94", "title-996");
        try
        {
            item.MoveTo(PrimaryTownHallRuleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryTownHallRuleStatus.Approved, "status");
    }

    public static void PrimaryTownHallRuleRejectsMovesFromWithdrawn()
    {
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("code-94", "title-996");
        item.Status = PrimaryTownHallRuleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PrimaryTownHallRuleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PrimaryTownHallRuleStartsInProposed();
        PrimaryTownHallRuleMovesToApproved();
        PrimaryTownHallRuleRejectsMovesFromWithdrawn();
    }
}
