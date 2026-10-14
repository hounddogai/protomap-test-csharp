namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleTarget;

using Acme.Workforce.Compensation.MeritCycle.CycleTarget;

/// <summary>Checks of cycle target code, run by the project's runner.</summary>
public static class MeritCycleBudgetChecks
{
    public static void MeritCycleBudgetStartsInProposed()
    {
        var item = MeritCycleBudget.NewMeritCycleBudget("code-242", "name-761");
        Check.Equal(item.Status, MeritCycleBudgetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleBudgetMovesToRejected()
    {
        var item = MeritCycleBudget.NewMeritCycleBudget("code-242", "name-761");
        try
        {
            item.MoveTo(MeritCycleBudgetStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleBudgetStatus.Rejected, "status");
    }

    public static void MeritCycleBudgetRejectsMovesFromWithdrawn()
    {
        var item = MeritCycleBudget.NewMeritCycleBudget("code-242", "name-761");
        item.Status = MeritCycleBudgetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(MeritCycleBudgetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        MeritCycleBudgetStartsInProposed();
        MeritCycleBudgetMovesToRejected();
        MeritCycleBudgetRejectsMovesFromWithdrawn();
    }
}
