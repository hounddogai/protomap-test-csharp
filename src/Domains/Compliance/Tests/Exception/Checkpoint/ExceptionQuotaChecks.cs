namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionQuotaChecks
{
    public static void ExceptionQuotaStartsInDraft()
    {
        var item = ExceptionQuota.NewExceptionQuota("code-670", "label-269");
        Check.Equal(item.Status, ExceptionQuotaStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionQuotaMovesToPending()
    {
        var item = ExceptionQuota.NewExceptionQuota("code-670", "label-269");
        try
        {
            item.MoveTo(ExceptionQuotaStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionQuotaStatus.Pending, "status");
    }

    public static void ExceptionQuotaRejectsMovesFromClosed()
    {
        var item = ExceptionQuota.NewExceptionQuota("code-670", "label-269");
        item.Status = ExceptionQuotaStatus.Closed;
        Check.Fails(() => item.MoveTo(ExceptionQuotaStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ExceptionQuotaStartsInDraft();
        ExceptionQuotaMovesToPending();
        ExceptionQuotaRejectsMovesFromClosed();
    }
}
