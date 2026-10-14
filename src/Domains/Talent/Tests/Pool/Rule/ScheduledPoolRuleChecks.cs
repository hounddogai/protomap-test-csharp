namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScheduledPoolRuleChecks
{
    public static void ScheduledPoolRuleStartsInNew()
    {
        var item = ScheduledPoolRule.NewScheduledPoolRule("code-284", "title-998");
        Check.Equal(item.Status, ScheduledPoolRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPoolRuleMovesToRunning()
    {
        var item = ScheduledPoolRule.NewScheduledPoolRule("code-284", "title-998");
        try
        {
            item.MoveTo(ScheduledPoolRuleStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPoolRuleStatus.Running, "status");
    }

    public static void ScheduledPoolRuleRejectsMovesFromFailed()
    {
        var item = ScheduledPoolRule.NewScheduledPoolRule("code-284", "title-998");
        item.Status = ScheduledPoolRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledPoolRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledPoolRuleStartsInNew();
        ScheduledPoolRuleMovesToRunning();
        ScheduledPoolRuleRejectsMovesFromFailed();
    }
}
