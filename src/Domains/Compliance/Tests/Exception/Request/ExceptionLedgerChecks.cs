namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionLedgerChecks
{
    public static void ExceptionLedgerStartsInPlanned()
    {
        var item = ExceptionLedger.NewExceptionLedger("code-455", "label-332");
        Check.Equal(item.Status, ExceptionLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionLedgerMovesToCompleted()
    {
        var item = ExceptionLedger.NewExceptionLedger("code-455", "label-332");
        try
        {
            item.MoveTo(ExceptionLedgerStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionLedgerStatus.Completed, "status");
    }

    public static void ExceptionLedgerRejectsMovesFromCancelled()
    {
        var item = ExceptionLedger.NewExceptionLedger("code-455", "label-332");
        item.Status = ExceptionLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ExceptionLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ExceptionLedgerStartsInPlanned();
        ExceptionLedgerMovesToCompleted();
        ExceptionLedgerRejectsMovesFromCancelled();
    }
}
