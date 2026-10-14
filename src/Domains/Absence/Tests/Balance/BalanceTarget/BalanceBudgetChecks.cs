namespace Acme.Workforce.Absence.Tests.Balance.BalanceTarget;

using Acme.Workforce.Absence.Balance.BalanceTarget;

/// <summary>Checks of balance target code, run by the project's runner.</summary>
public static class BalanceBudgetChecks
{
    public static void BalanceBudgetStartsInPlanned()
    {
        var item = BalanceBudget.NewBalanceBudget("code-352", "label-886");
        Check.Equal(item.Status, BalanceBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceBudgetMovesToCompleted()
    {
        var item = BalanceBudget.NewBalanceBudget("code-352", "label-886");
        try
        {
            item.MoveTo(BalanceBudgetStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceBudgetStatus.Completed, "status");
    }

    public static void BalanceBudgetRejectsMovesFromCancelled()
    {
        var item = BalanceBudget.NewBalanceBudget("code-352", "label-886");
        item.Status = BalanceBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BalanceBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BalanceBudgetStartsInPlanned();
        BalanceBudgetMovesToCompleted();
        BalanceBudgetRejectsMovesFromCancelled();
    }
}
