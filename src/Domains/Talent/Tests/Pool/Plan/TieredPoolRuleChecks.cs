namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TieredPoolRuleChecks
{
    public static void TieredPoolRuleStartsInNew()
    {
        var item = TieredPoolRule.NewTieredPoolRule("code-36", "label-451");
        Check.Equal(item.Status, TieredPoolRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPoolRuleMovesToFailed()
    {
        var item = TieredPoolRule.NewTieredPoolRule("code-36", "label-451");
        try
        {
            item.MoveTo(TieredPoolRuleStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPoolRuleStatus.Failed, "status");
    }

    public static void TieredPoolRuleRejectsMovesFromFailed()
    {
        var item = TieredPoolRule.NewTieredPoolRule("code-36", "label-451");
        item.Status = TieredPoolRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(TieredPoolRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        TieredPoolRuleStartsInNew();
        TieredPoolRuleMovesToFailed();
        TieredPoolRuleRejectsMovesFromFailed();
    }
}
