namespace Acme.Workforce.Workplace.Tests.Occupancy.Ledger;

using Acme.Workforce.Workplace.Occupancy.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class OccupancyLedgerChecks
{
    public static void OccupancyLedgerStartsInPlanned()
    {
        var item = OccupancyLedger.NewOccupancyLedger("code-768", "title-140");
        Check.Equal(item.Status, OccupancyLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyLedgerMovesToScheduled()
    {
        var item = OccupancyLedger.NewOccupancyLedger("code-768", "title-140");
        try
        {
            item.MoveTo(OccupancyLedgerStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyLedgerStatus.Scheduled, "status");
    }

    public static void OccupancyLedgerRejectsMovesFromCancelled()
    {
        var item = OccupancyLedger.NewOccupancyLedger("code-768", "title-140");
        item.Status = OccupancyLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OccupancyLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OccupancyLedgerStartsInPlanned();
        OccupancyLedgerMovesToScheduled();
        OccupancyLedgerRejectsMovesFromCancelled();
    }
}
