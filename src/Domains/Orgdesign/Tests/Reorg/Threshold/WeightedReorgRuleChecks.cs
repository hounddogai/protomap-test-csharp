namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class WeightedReorgRuleChecks
{
    public static void WeightedReorgRuleStartsInProposed()
    {
        var item = WeightedReorgRule.NewWeightedReorgRule("code-370", "name-700");
        Check.Equal(item.Status, WeightedReorgRuleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedReorgRuleMovesToRejected()
    {
        var item = WeightedReorgRule.NewWeightedReorgRule("code-370", "name-700");
        try
        {
            item.MoveTo(WeightedReorgRuleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedReorgRuleStatus.Rejected, "status");
    }

    public static void WeightedReorgRuleRejectsMovesFromWithdrawn()
    {
        var item = WeightedReorgRule.NewWeightedReorgRule("code-370", "name-700");
        item.Status = WeightedReorgRuleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedReorgRuleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedReorgRuleStartsInProposed();
        WeightedReorgRuleMovesToRejected();
        WeightedReorgRuleRejectsMovesFromWithdrawn();
    }
}
