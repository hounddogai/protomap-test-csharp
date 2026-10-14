namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceLedgerChecks
{
    public static void BalanceLedgerStartsInPlanned()
    {
        var item = BalanceLedger.NewBalanceLedger("code-500", "label-216");
        Check.Equal(item.Status, BalanceLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BalanceLedgerMovesToCompleted()
    {
        var item = BalanceLedger.NewBalanceLedger("code-500", "label-216");
        try
        {
            item.MoveTo(BalanceLedgerStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BalanceLedgerStatus.Completed, "status");
    }

    public static void BalanceLedgerRejectsMovesFromCancelled()
    {
        var item = BalanceLedger.NewBalanceLedger("code-500", "label-216");
        item.Status = BalanceLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BalanceLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BalanceLedgerStartsInPlanned();
        BalanceLedgerMovesToCompleted();
        BalanceLedgerRejectsMovesFromCancelled();
    }
}
