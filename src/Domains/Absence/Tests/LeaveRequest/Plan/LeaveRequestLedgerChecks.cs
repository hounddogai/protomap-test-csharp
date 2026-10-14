namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LeaveRequestLedgerChecks
{
    public static void LeaveRequestLedgerStartsInOpen()
    {
        var item = LeaveRequestLedger.NewLeaveRequestLedger("code-890", "name-275");
        Check.Equal(item.Status, LeaveRequestLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestLedgerMovesToInReview()
    {
        var item = LeaveRequestLedger.NewLeaveRequestLedger("code-890", "name-275");
        try
        {
            item.MoveTo(LeaveRequestLedgerStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestLedgerStatus.InReview, "status");
    }

    public static void LeaveRequestLedgerRejectsMovesFromArchived()
    {
        var item = LeaveRequestLedger.NewLeaveRequestLedger("code-890", "name-275");
        item.Status = LeaveRequestLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(LeaveRequestLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        LeaveRequestLedgerStartsInOpen();
        LeaveRequestLedgerMovesToInReview();
        LeaveRequestLedgerRejectsMovesFromArchived();
    }
}
