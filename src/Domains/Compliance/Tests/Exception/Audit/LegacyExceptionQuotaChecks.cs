namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyExceptionQuotaChecks
{
    public static void LegacyExceptionQuotaStartsInDraft()
    {
        var item = LegacyExceptionQuota.NewLegacyExceptionQuota("code-780", "title-762");
        Check.Equal(item.Status, LegacyExceptionQuotaStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyExceptionQuotaMovesToSuspended()
    {
        var item = LegacyExceptionQuota.NewLegacyExceptionQuota("code-780", "title-762");
        try
        {
            item.MoveTo(LegacyExceptionQuotaStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyExceptionQuotaStatus.Suspended, "status");
    }

    public static void LegacyExceptionQuotaRejectsMovesFromClosed()
    {
        var item = LegacyExceptionQuota.NewLegacyExceptionQuota("code-780", "title-762");
        item.Status = LegacyExceptionQuotaStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyExceptionQuotaStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyExceptionQuotaStartsInDraft();
        LegacyExceptionQuotaMovesToSuspended();
        LegacyExceptionQuotaRejectsMovesFromClosed();
    }
}
