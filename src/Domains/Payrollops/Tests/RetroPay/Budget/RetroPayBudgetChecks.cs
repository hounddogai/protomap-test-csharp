namespace Acme.Workforce.Payrollops.Tests.RetroPay.Budget;

using Acme.Workforce.Payrollops.RetroPay.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RetroPayBudgetChecks
{
    public static void RetroPayBudgetStartsInPlanned()
    {
        var item = RetroPayBudget.NewRetroPayBudget("code-911", "label-202");
        Check.Equal(item.Status, RetroPayBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayBudgetMovesToInProgress()
    {
        var item = RetroPayBudget.NewRetroPayBudget("code-911", "label-202");
        try
        {
            item.MoveTo(RetroPayBudgetStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayBudgetStatus.InProgress, "status");
    }

    public static void RetroPayBudgetRejectsMovesFromCancelled()
    {
        var item = RetroPayBudget.NewRetroPayBudget("code-911", "label-202");
        item.Status = RetroPayBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RetroPayBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RetroPayBudgetStartsInPlanned();
        RetroPayBudgetMovesToInProgress();
        RetroPayBudgetRejectsMovesFromCancelled();
    }
}
