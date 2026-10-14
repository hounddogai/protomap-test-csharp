namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveLeaveRequestLedgerChecks
{
    public static void AdaptiveLeaveRequestLedgerStartsInPlanned()
    {
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("code-353", "name-720");
        Check.Equal(item.Status, AdaptiveLeaveRequestLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveLeaveRequestLedgerMovesToScheduled()
    {
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("code-353", "name-720");
        try
        {
            item.MoveTo(AdaptiveLeaveRequestLedgerStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveLeaveRequestLedgerStatus.Scheduled, "status");
    }

    public static void AdaptiveLeaveRequestLedgerRejectsMovesFromCancelled()
    {
        var item = AdaptiveLeaveRequestLedger.NewAdaptiveLeaveRequestLedger("code-353", "name-720");
        item.Status = AdaptiveLeaveRequestLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveLeaveRequestLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveLeaveRequestLedgerStartsInPlanned();
        AdaptiveLeaveRequestLedgerMovesToScheduled();
        AdaptiveLeaveRequestLedgerRejectsMovesFromCancelled();
    }
}
