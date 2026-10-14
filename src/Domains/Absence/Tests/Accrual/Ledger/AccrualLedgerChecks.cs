namespace Acme.Workforce.Absence.Tests.Accrual.Ledger;

using Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AccrualLedgerChecks
{
    public static void AccrualLedgerStartsInPlanned()
    {
        var item = AccrualLedger.NewAccrualLedger("code-636", "label-913");
        Check.Equal(item.Status, AccrualLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualLedgerMovesToInProgress()
    {
        var item = AccrualLedger.NewAccrualLedger("code-636", "label-913");
        try
        {
            item.MoveTo(AccrualLedgerStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualLedgerStatus.InProgress, "status");
    }

    public static void AccrualLedgerRejectsMovesFromCancelled()
    {
        var item = AccrualLedger.NewAccrualLedger("code-636", "label-913");
        item.Status = AccrualLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AccrualLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AccrualLedgerStartsInPlanned();
        AccrualLedgerMovesToInProgress();
        AccrualLedgerRejectsMovesFromCancelled();
    }
}
