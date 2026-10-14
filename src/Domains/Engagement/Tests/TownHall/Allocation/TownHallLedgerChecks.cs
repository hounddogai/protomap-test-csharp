namespace Acme.Workforce.Engagement.Tests.TownHall.Allocation;

using Acme.Workforce.Engagement.TownHall.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TownHallLedgerChecks
{
    public static void TownHallLedgerStartsInProposed()
    {
        var item = TownHallLedger.NewTownHallLedger("code-585", "name-493");
        Check.Equal(item.Status, TownHallLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallLedgerMovesToRejected()
    {
        var item = TownHallLedger.NewTownHallLedger("code-585", "name-493");
        try
        {
            item.MoveTo(TownHallLedgerStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallLedgerStatus.Rejected, "status");
    }

    public static void TownHallLedgerRejectsMovesFromWithdrawn()
    {
        var item = TownHallLedger.NewTownHallLedger("code-585", "name-493");
        item.Status = TownHallLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TownHallLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TownHallLedgerStartsInProposed();
        TownHallLedgerMovesToRejected();
        TownHallLedgerRejectsMovesFromWithdrawn();
    }
}
