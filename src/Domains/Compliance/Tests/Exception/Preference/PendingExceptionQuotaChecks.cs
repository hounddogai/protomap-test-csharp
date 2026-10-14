namespace Acme.Workforce.Compliance.Tests.Exception.Preference;

using Acme.Workforce.Compliance.Exception.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PendingExceptionQuotaChecks
{
    public static void PendingExceptionQuotaStartsInCreated()
    {
        var item = PendingExceptionQuota.NewPendingExceptionQuota("code-599", "name-932");
        Check.Equal(item.Status, PendingExceptionQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingExceptionQuotaMovesToValidated()
    {
        var item = PendingExceptionQuota.NewPendingExceptionQuota("code-599", "name-932");
        try
        {
            item.MoveTo(PendingExceptionQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingExceptionQuotaStatus.Validated, "status");
    }

    public static void PendingExceptionQuotaRejectsMovesFromRetired()
    {
        var item = PendingExceptionQuota.NewPendingExceptionQuota("code-599", "name-932");
        item.Status = PendingExceptionQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(PendingExceptionQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        PendingExceptionQuotaStartsInCreated();
        PendingExceptionQuotaMovesToValidated();
        PendingExceptionQuotaRejectsMovesFromRetired();
    }
}
