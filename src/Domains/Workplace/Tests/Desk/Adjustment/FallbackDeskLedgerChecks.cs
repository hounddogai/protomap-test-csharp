namespace Acme.Workforce.Workplace.Tests.Desk.Adjustment;

using Acme.Workforce.Workplace.Desk.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FallbackDeskLedgerChecks
{
    public static void FallbackDeskLedgerStartsInProposed()
    {
        var item = FallbackDeskLedger.NewFallbackDeskLedger("code-993", "name-293");
        Check.Equal(item.Status, FallbackDeskLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackDeskLedgerMovesToRejected()
    {
        var item = FallbackDeskLedger.NewFallbackDeskLedger("code-993", "name-293");
        try
        {
            item.MoveTo(FallbackDeskLedgerStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackDeskLedgerStatus.Rejected, "status");
    }

    public static void FallbackDeskLedgerRejectsMovesFromWithdrawn()
    {
        var item = FallbackDeskLedger.NewFallbackDeskLedger("code-993", "name-293");
        item.Status = FallbackDeskLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackDeskLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackDeskLedgerStartsInProposed();
        FallbackDeskLedgerMovesToRejected();
        FallbackDeskLedgerRejectsMovesFromWithdrawn();
    }
}
