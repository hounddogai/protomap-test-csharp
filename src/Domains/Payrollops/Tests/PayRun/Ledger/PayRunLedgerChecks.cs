namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PayRunLedgerChecks
{
    public static void PayRunLedgerStartsInCreated()
    {
        var item = PayRunLedger.NewPayRunLedger("code-723", "name-198");
        Check.Equal(item.Status, PayRunLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunLedgerMovesToValidated()
    {
        var item = PayRunLedger.NewPayRunLedger("code-723", "name-198");
        try
        {
            item.MoveTo(PayRunLedgerStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunLedgerStatus.Validated, "status");
    }

    public static void PayRunLedgerRejectsMovesFromRetired()
    {
        var item = PayRunLedger.NewPayRunLedger("code-723", "name-198");
        item.Status = PayRunLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(PayRunLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayRunLedgerStartsInCreated();
        PayRunLedgerMovesToValidated();
        PayRunLedgerRejectsMovesFromRetired();
    }
}
