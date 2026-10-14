namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class DefaultPayRunLedgerChecks
{
    public static void DefaultPayRunLedgerStartsInIdle()
    {
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("code-455", "title-272");
        Check.Equal(item.Status, DefaultPayRunLedgerStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultPayRunLedgerMovesToWarming()
    {
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("code-455", "title-272");
        try
        {
            item.MoveTo(DefaultPayRunLedgerStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultPayRunLedgerStatus.Warming, "status");
    }

    public static void DefaultPayRunLedgerRejectsMovesFromStopped()
    {
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("code-455", "title-272");
        item.Status = DefaultPayRunLedgerStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultPayRunLedgerStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultPayRunLedgerStartsInIdle();
        DefaultPayRunLedgerMovesToWarming();
        DefaultPayRunLedgerRejectsMovesFromStopped();
    }
}
