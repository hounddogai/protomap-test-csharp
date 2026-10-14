namespace Acme.Workforce.Orgdesign.Tests.CostModel.Score;

using Acme.Workforce.Orgdesign.CostModel.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class CostModelRuleChecks
{
    public static void CostModelRuleStartsInRequested()
    {
        var item = CostModelRule.NewCostModelRule("code-674", "label-140");
        Check.Equal(item.Status, CostModelRuleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelRuleMovesToGranted()
    {
        var item = CostModelRule.NewCostModelRule("code-674", "label-140");
        try
        {
            item.MoveTo(CostModelRuleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelRuleStatus.Granted, "status");
    }

    public static void CostModelRuleRejectsMovesFromExpired()
    {
        var item = CostModelRule.NewCostModelRule("code-674", "label-140");
        item.Status = CostModelRuleStatus.Expired;
        Check.Fails(() => item.MoveTo(CostModelRuleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CostModelRuleStartsInRequested();
        CostModelRuleMovesToGranted();
        CostModelRuleRejectsMovesFromExpired();
    }
}
