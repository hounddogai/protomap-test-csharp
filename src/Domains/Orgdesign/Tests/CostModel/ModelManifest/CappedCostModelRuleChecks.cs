namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelManifest;

using Acme.Workforce.Orgdesign.CostModel.ModelManifest;

/// <summary>Checks of model manifest code, run by the project's runner.</summary>
public static class CappedCostModelRuleChecks
{
    public static void CappedCostModelRuleStartsInOpen()
    {
        var item = CappedCostModelRule.NewCappedCostModelRule("code-328", "name-873");
        Check.Equal(item.Status, CappedCostModelRuleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedCostModelRuleMovesToReopened()
    {
        var item = CappedCostModelRule.NewCappedCostModelRule("code-328", "name-873");
        try
        {
            item.MoveTo(CappedCostModelRuleStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedCostModelRuleStatus.Reopened, "status");
    }

    public static void CappedCostModelRuleRejectsMovesFromArchived()
    {
        var item = CappedCostModelRule.NewCappedCostModelRule("code-328", "name-873");
        item.Status = CappedCostModelRuleStatus.Archived;
        Check.Fails(() => item.MoveTo(CappedCostModelRuleStatus.Open), "move");
    }

    public static void RunAll()
    {
        CappedCostModelRuleStartsInOpen();
        CappedCostModelRuleMovesToReopened();
        CappedCostModelRuleRejectsMovesFromArchived();
    }
}
