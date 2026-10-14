namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantBudgetChecks
{
    public static void EquityGrantBudgetStartsInProposed()
    {
        var item = EquityGrantBudget.NewEquityGrantBudget("code-369", "label-18");
        Check.Equal(item.Status, EquityGrantBudgetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantBudgetMovesToRejected()
    {
        var item = EquityGrantBudget.NewEquityGrantBudget("code-369", "label-18");
        try
        {
            item.MoveTo(EquityGrantBudgetStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantBudgetStatus.Rejected, "status");
    }

    public static void EquityGrantBudgetRejectsMovesFromWithdrawn()
    {
        var item = EquityGrantBudget.NewEquityGrantBudget("code-369", "label-18");
        item.Status = EquityGrantBudgetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(EquityGrantBudgetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        EquityGrantBudgetStartsInProposed();
        EquityGrantBudgetMovesToRejected();
        EquityGrantBudgetRejectsMovesFromWithdrawn();
    }
}
