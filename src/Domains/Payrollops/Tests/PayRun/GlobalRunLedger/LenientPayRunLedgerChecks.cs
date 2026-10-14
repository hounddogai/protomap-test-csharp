namespace Acme.Workforce.Payrollops.Tests.PayRun.GlobalRunLedger;

using Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

/// <summary>Checks of global run ledger code, run by the project's runner.</summary>
public static class LenientPayRunLedgerChecks
{
    public static void LenientPayRunLedgerStartsInProposed()
    {
        var item = LenientPayRunLedger.NewLenientPayRunLedger("code-975", "name-288");
        Check.Equal(item.Status, LenientPayRunLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPayRunLedgerMovesToRejected()
    {
        var item = LenientPayRunLedger.NewLenientPayRunLedger("code-975", "name-288");
        try
        {
            item.MoveTo(LenientPayRunLedgerStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPayRunLedgerStatus.Rejected, "status");
    }

    public static void LenientPayRunLedgerRejectsMovesFromWithdrawn()
    {
        var item = LenientPayRunLedger.NewLenientPayRunLedger("code-975", "name-288");
        item.Status = LenientPayRunLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientPayRunLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientPayRunLedgerStartsInProposed();
        LenientPayRunLedgerMovesToRejected();
        LenientPayRunLedgerRejectsMovesFromWithdrawn();
    }
}
