namespace Acme.Workforce.Payrollops.Tests.Garnishment.Ledger;

using Acme.Workforce.Payrollops.Garnishment.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GarnishmentLedgerChecks
{
    public static void GarnishmentLedgerStartsInProposed()
    {
        var item = GarnishmentLedger.NewGarnishmentLedger("code-673", "name-218");
        Check.Equal(item.Status, GarnishmentLedgerStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GarnishmentLedgerMovesToApproved()
    {
        var item = GarnishmentLedger.NewGarnishmentLedger("code-673", "name-218");
        try
        {
            item.MoveTo(GarnishmentLedgerStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GarnishmentLedgerStatus.Approved, "status");
    }

    public static void GarnishmentLedgerRejectsMovesFromWithdrawn()
    {
        var item = GarnishmentLedger.NewGarnishmentLedger("code-673", "name-218");
        item.Status = GarnishmentLedgerStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GarnishmentLedgerStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GarnishmentLedgerStartsInProposed();
        GarnishmentLedgerMovesToApproved();
        GarnishmentLedgerRejectsMovesFromWithdrawn();
    }
}
