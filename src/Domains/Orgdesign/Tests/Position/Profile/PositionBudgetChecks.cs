namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PositionBudgetChecks
{
    public static void PositionBudgetStartsInProposed()
    {
        var item = PositionBudget.NewPositionBudget("code-689", "name-334");
        Check.Equal(item.Status, PositionBudgetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionBudgetMovesToApproved()
    {
        var item = PositionBudget.NewPositionBudget("code-689", "name-334");
        try
        {
            item.MoveTo(PositionBudgetStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionBudgetStatus.Approved, "status");
    }

    public static void PositionBudgetRejectsMovesFromWithdrawn()
    {
        var item = PositionBudget.NewPositionBudget("code-689", "name-334");
        item.Status = PositionBudgetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PositionBudgetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PositionBudgetStartsInProposed();
        PositionBudgetMovesToApproved();
        PositionBudgetRejectsMovesFromWithdrawn();
    }
}
