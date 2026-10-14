namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayQuotaChecks
{
    public static void RetroPayQuotaStartsInOpen()
    {
        var item = RetroPayQuota.NewRetroPayQuota("code-261", "name-747");
        Check.Equal(item.Status, RetroPayQuotaStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayQuotaMovesToResolved()
    {
        var item = RetroPayQuota.NewRetroPayQuota("code-261", "name-747");
        try
        {
            item.MoveTo(RetroPayQuotaStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayQuotaStatus.Resolved, "status");
    }

    public static void RetroPayQuotaRejectsMovesFromArchived()
    {
        var item = RetroPayQuota.NewRetroPayQuota("code-261", "name-747");
        item.Status = RetroPayQuotaStatus.Archived;
        Check.Fails(() => item.MoveTo(RetroPayQuotaStatus.Open), "move");
    }

    public static void RunAll()
    {
        RetroPayQuotaStartsInOpen();
        RetroPayQuotaMovesToResolved();
        RetroPayQuotaRejectsMovesFromArchived();
    }
}
