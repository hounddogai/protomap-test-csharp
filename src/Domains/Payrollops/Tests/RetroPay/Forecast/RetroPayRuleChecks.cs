namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayRuleChecks
{
    public static void RetroPayRuleStartsInPlanned()
    {
        var item = RetroPayRule.NewRetroPayRule("code-574", "title-262");
        Check.Equal(item.Status, RetroPayRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayRuleMovesToCompleted()
    {
        var item = RetroPayRule.NewRetroPayRule("code-574", "title-262");
        try
        {
            item.MoveTo(RetroPayRuleStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayRuleStatus.Completed, "status");
    }

    public static void RetroPayRuleRejectsMovesFromCancelled()
    {
        var item = RetroPayRule.NewRetroPayRule("code-574", "title-262");
        item.Status = RetroPayRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RetroPayRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RetroPayRuleStartsInPlanned();
        RetroPayRuleMovesToCompleted();
        RetroPayRuleRejectsMovesFromCancelled();
    }
}
