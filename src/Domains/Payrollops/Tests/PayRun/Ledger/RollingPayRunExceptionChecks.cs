namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RollingPayRunExceptionChecks
{
    public static void RollingPayRunExceptionStartsInDraft()
    {
        var item = RollingPayRunException.NewRollingPayRunException("code-628", "name-722");
        Check.Equal(item.Status, RollingPayRunExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPayRunExceptionMovesToSuspended()
    {
        var item = RollingPayRunException.NewRollingPayRunException("code-628", "name-722");
        try
        {
            item.MoveTo(RollingPayRunExceptionStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPayRunExceptionStatus.Suspended, "status");
    }

    public static void RollingPayRunExceptionRejectsMovesFromClosed()
    {
        var item = RollingPayRunException.NewRollingPayRunException("code-628", "name-722");
        item.Status = RollingPayRunExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingPayRunExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingPayRunExceptionStartsInDraft();
        RollingPayRunExceptionMovesToSuspended();
        RollingPayRunExceptionRejectsMovesFromClosed();
    }
}
