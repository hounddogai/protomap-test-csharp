namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ExceptionRuleChecks
{
    public static void ExceptionRuleStartsInPlanned()
    {
        var item = ExceptionRule.NewExceptionRule("code-537", "title-822");
        Check.Equal(item.Status, ExceptionRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionRuleMovesToScheduled()
    {
        var item = ExceptionRule.NewExceptionRule("code-537", "title-822");
        try
        {
            item.MoveTo(ExceptionRuleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionRuleStatus.Scheduled, "status");
    }

    public static void ExceptionRuleRejectsMovesFromCancelled()
    {
        var item = ExceptionRule.NewExceptionRule("code-537", "title-822");
        item.Status = ExceptionRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ExceptionRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ExceptionRuleStartsInPlanned();
        ExceptionRuleMovesToScheduled();
        ExceptionRuleRejectsMovesFromCancelled();
    }
}
