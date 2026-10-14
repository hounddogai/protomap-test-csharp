namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingAccrualWindowChecks
{
    public static void PendingAccrualWindowStartsInProposed()
    {
        var item = PendingAccrualWindow.NewPendingAccrualWindow("code-930", "title-473");
        Check.Equal(item.Status, PendingAccrualWindowStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAccrualWindowMovesToWithdrawn()
    {
        var item = PendingAccrualWindow.NewPendingAccrualWindow("code-930", "title-473");
        try
        {
            item.MoveTo(PendingAccrualWindowStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAccrualWindowStatus.Withdrawn, "status");
    }

    public static void PendingAccrualWindowRejectsMovesFromWithdrawn()
    {
        var item = PendingAccrualWindow.NewPendingAccrualWindow("code-930", "title-473");
        item.Status = PendingAccrualWindowStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingAccrualWindowStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingAccrualWindowStartsInProposed();
        PendingAccrualWindowMovesToWithdrawn();
        PendingAccrualWindowRejectsMovesFromWithdrawn();
    }
}
