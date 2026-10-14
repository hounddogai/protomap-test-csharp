namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptiveTownHallLedgerChecks
{
    public static void AdaptiveTownHallLedgerStartsInProposed()
    {
        var item = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger("code-324", "name-96");
        Check.Equal(item.Status, AdaptiveTownHallLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTownHallLedgerMovesToRejected()
    {
        var item = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger("code-324", "name-96");
        try
        {
            item.MoveTo(AdaptiveTownHallLedgerStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTownHallLedgerStatus.Rejected, "status");
    }

    public static void AdaptiveTownHallLedgerRejectsMovesFromWithdrawn()
    {
        var item = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger("code-324", "name-96");
        item.Status = AdaptiveTownHallLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AdaptiveTownHallLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AdaptiveTownHallLedgerStartsInProposed();
        AdaptiveTownHallLedgerMovesToRejected();
        AdaptiveTownHallLedgerRejectsMovesFromWithdrawn();
    }
}
