namespace Acme.Workforce.Talent.Tests.Pool.Budget;

using Acme.Workforce.Talent.Pool.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PoolBudgetChecks
{
    public static void PoolBudgetStartsInRequested()
    {
        var item = PoolBudget.NewPoolBudget("code-372", "name-376");
        Check.Equal(item.Status, PoolBudgetStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolBudgetMovesToGranted()
    {
        var item = PoolBudget.NewPoolBudget("code-372", "name-376");
        try
        {
            item.MoveTo(PoolBudgetStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolBudgetStatus.Granted, "status");
    }

    public static void PoolBudgetRejectsMovesFromExpired()
    {
        var item = PoolBudget.NewPoolBudget("code-372", "name-376");
        item.Status = PoolBudgetStatus.Expired;
        Check.Fails(() => item.MoveTo(PoolBudgetStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PoolBudgetStartsInRequested();
        PoolBudgetMovesToGranted();
        PoolBudgetRejectsMovesFromExpired();
    }
}
