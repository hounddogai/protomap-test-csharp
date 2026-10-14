namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledExceptionLedgerChecks
{
    public static void ScheduledExceptionLedgerStartsInDraft()
    {
        var item = ScheduledExceptionLedger.NewScheduledExceptionLedger("code-974", "name-1");
        Check.Equal(item.Status, ScheduledExceptionLedgerStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledExceptionLedgerMovesToActive()
    {
        var item = ScheduledExceptionLedger.NewScheduledExceptionLedger("code-974", "name-1");
        try
        {
            item.MoveTo(ScheduledExceptionLedgerStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledExceptionLedgerStatus.Active, "status");
    }

    public static void ScheduledExceptionLedgerRejectsMovesFromClosed()
    {
        var item = ScheduledExceptionLedger.NewScheduledExceptionLedger("code-974", "name-1");
        item.Status = ScheduledExceptionLedgerStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledExceptionLedgerStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledExceptionLedgerStartsInDraft();
        ScheduledExceptionLedgerMovesToActive();
        ScheduledExceptionLedgerRejectsMovesFromClosed();
    }
}
