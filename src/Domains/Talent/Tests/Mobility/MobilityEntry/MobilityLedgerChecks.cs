namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class MobilityLedgerChecks
{
    public static void MobilityLedgerStartsInPlanned()
    {
        var item = MobilityLedger.NewMobilityLedger("code-226", "title-380");
        Check.Equal(item.Status, MobilityLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityLedgerMovesToInProgress()
    {
        var item = MobilityLedger.NewMobilityLedger("code-226", "title-380");
        try
        {
            item.MoveTo(MobilityLedgerStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityLedgerStatus.InProgress, "status");
    }

    public static void MobilityLedgerRejectsMovesFromCancelled()
    {
        var item = MobilityLedger.NewMobilityLedger("code-226", "title-380");
        item.Status = MobilityLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(MobilityLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        MobilityLedgerStartsInPlanned();
        MobilityLedgerMovesToInProgress();
        MobilityLedgerRejectsMovesFromCancelled();
    }
}
