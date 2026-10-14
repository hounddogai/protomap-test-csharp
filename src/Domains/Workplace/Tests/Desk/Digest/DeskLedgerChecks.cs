namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DeskLedgerChecks
{
    public static void DeskLedgerStartsInProposed()
    {
        var item = DeskLedger.NewDeskLedger("code-696", "title-260");
        Check.Equal(item.Status, DeskLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskLedgerMovesToApproved()
    {
        var item = DeskLedger.NewDeskLedger("code-696", "title-260");
        try
        {
            item.MoveTo(DeskLedgerStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskLedgerStatus.Approved, "status");
    }

    public static void DeskLedgerRejectsMovesFromWithdrawn()
    {
        var item = DeskLedger.NewDeskLedger("code-696", "title-260");
        item.Status = DeskLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeskLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeskLedgerStartsInProposed();
        DeskLedgerMovesToApproved();
        DeskLedgerRejectsMovesFromWithdrawn();
    }
}
