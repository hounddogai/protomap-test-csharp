namespace Acme.Workforce.Orgdesign.Tests.CostModel.Rule;

using Acme.Workforce.Orgdesign.CostModel.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PendingCostModelRuleChecks
{
    public static void PendingCostModelRuleStartsInNew()
    {
        var item = PendingCostModelRule.NewPendingCostModelRule("code-975", "label-985");
        Check.Equal(item.Status, PendingCostModelRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingCostModelRuleMovesToFailed()
    {
        var item = PendingCostModelRule.NewPendingCostModelRule("code-975", "label-985");
        try
        {
            item.MoveTo(PendingCostModelRuleStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingCostModelRuleStatus.Failed, "status");
    }

    public static void PendingCostModelRuleRejectsMovesFromFailed()
    {
        var item = PendingCostModelRule.NewPendingCostModelRule("code-975", "label-985");
        item.Status = PendingCostModelRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingCostModelRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingCostModelRuleStartsInNew();
        PendingCostModelRuleMovesToFailed();
        PendingCostModelRuleRejectsMovesFromFailed();
    }
}
