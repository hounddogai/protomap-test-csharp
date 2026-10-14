namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class SeasonalAccrualLedgerChecks
{
    public static void SeasonalAccrualLedgerStartsInIdle()
    {
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("code-223", "title-998");
        Check.Equal(item.Status, SeasonalAccrualLedgerStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAccrualLedgerMovesToDraining()
    {
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("code-223", "title-998");
        try
        {
            item.MoveTo(SeasonalAccrualLedgerStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAccrualLedgerStatus.Draining, "status");
    }

    public static void SeasonalAccrualLedgerRejectsMovesFromStopped()
    {
        var item = SeasonalAccrualLedger.NewSeasonalAccrualLedger("code-223", "title-998");
        item.Status = SeasonalAccrualLedgerStatus.Stopped;
        Check.Fails(() => item.MoveTo(SeasonalAccrualLedgerStatus.Idle), "move");
    }

    public static void RunAll()
    {
        SeasonalAccrualLedgerStartsInIdle();
        SeasonalAccrualLedgerMovesToDraining();
        SeasonalAccrualLedgerRejectsMovesFromStopped();
    }
}
