namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PendingRetroPayRuleChecks
{
    public static void PendingRetroPayRuleStartsInPlanned()
    {
        var item = PendingRetroPayRule.NewPendingRetroPayRule("code-732", "name-899");
        Check.Equal(item.Status, PendingRetroPayRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingRetroPayRuleMovesToInProgress()
    {
        var item = PendingRetroPayRule.NewPendingRetroPayRule("code-732", "name-899");
        try
        {
            item.MoveTo(PendingRetroPayRuleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingRetroPayRuleStatus.InProgress, "status");
    }

    public static void PendingRetroPayRuleRejectsMovesFromCancelled()
    {
        var item = PendingRetroPayRule.NewPendingRetroPayRule("code-732", "name-899");
        item.Status = PendingRetroPayRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PendingRetroPayRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PendingRetroPayRuleStartsInPlanned();
        PendingRetroPayRuleMovesToInProgress();
        PendingRetroPayRuleRejectsMovesFromCancelled();
    }
}
