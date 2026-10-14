namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorRuleChecks
{
    public static void VisitorRuleStartsInIdle()
    {
        var item = VisitorRule.NewVisitorRule("code-632", "title-328");
        Check.Equal(item.Status, VisitorRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorRuleMovesToReady()
    {
        var item = VisitorRule.NewVisitorRule("code-632", "title-328");
        try
        {
            item.MoveTo(VisitorRuleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorRuleStatus.Ready, "status");
    }

    public static void VisitorRuleRejectsMovesFromStopped()
    {
        var item = VisitorRule.NewVisitorRule("code-632", "title-328");
        item.Status = VisitorRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(VisitorRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        VisitorRuleStartsInIdle();
        VisitorRuleMovesToReady();
        VisitorRuleRejectsMovesFromStopped();
    }
}
