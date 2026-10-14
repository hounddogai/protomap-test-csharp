namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DefaultExceptionQuotaChecks
{
    public static void DefaultExceptionQuotaStartsInPlanned()
    {
        var item = DefaultExceptionQuota.NewDefaultExceptionQuota("code-293", "title-62");
        Check.Equal(item.Status, DefaultExceptionQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultExceptionQuotaMovesToScheduled()
    {
        var item = DefaultExceptionQuota.NewDefaultExceptionQuota("code-293", "title-62");
        try
        {
            item.MoveTo(DefaultExceptionQuotaStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultExceptionQuotaStatus.Scheduled, "status");
    }

    public static void DefaultExceptionQuotaRejectsMovesFromCancelled()
    {
        var item = DefaultExceptionQuota.NewDefaultExceptionQuota("code-293", "title-62");
        item.Status = DefaultExceptionQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultExceptionQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultExceptionQuotaStartsInPlanned();
        DefaultExceptionQuotaMovesToScheduled();
        DefaultExceptionQuotaRejectsMovesFromCancelled();
    }
}
