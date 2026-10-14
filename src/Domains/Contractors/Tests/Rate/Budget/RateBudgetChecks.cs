namespace Acme.Workforce.Contractors.Tests.Rate.Budget;

using Acme.Workforce.Contractors.Rate.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class RateBudgetChecks
{
    public static void RateBudgetStartsInProposed()
    {
        var item = RateBudget.NewRateBudget("code-20", "name-133");
        Check.Equal(item.Status, RateBudgetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateBudgetMovesToApproved()
    {
        var item = RateBudget.NewRateBudget("code-20", "name-133");
        try
        {
            item.MoveTo(RateBudgetStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateBudgetStatus.Approved, "status");
    }

    public static void RateBudgetRejectsMovesFromWithdrawn()
    {
        var item = RateBudget.NewRateBudget("code-20", "name-133");
        item.Status = RateBudgetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RateBudgetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RateBudgetStartsInProposed();
        RateBudgetMovesToApproved();
        RateBudgetRejectsMovesFromWithdrawn();
    }
}
