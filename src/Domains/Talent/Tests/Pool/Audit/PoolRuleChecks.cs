namespace Acme.Workforce.Talent.Tests.Pool.Audit;

using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PoolRuleChecks
{
    public static void PoolRuleStartsInNew()
    {
        var item = PoolRule.NewPoolRule("code-950", "label-949");
        Check.Equal(item.Status, PoolRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolRuleMovesToRunning()
    {
        var item = PoolRule.NewPoolRule("code-950", "label-949");
        try
        {
            item.MoveTo(PoolRuleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolRuleStatus.Running, "status");
    }

    public static void PoolRuleRejectsMovesFromFailed()
    {
        var item = PoolRule.NewPoolRule("code-950", "label-949");
        item.Status = PoolRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(PoolRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        PoolRuleStartsInNew();
        PoolRuleMovesToRunning();
        PoolRuleRejectsMovesFromFailed();
    }
}
