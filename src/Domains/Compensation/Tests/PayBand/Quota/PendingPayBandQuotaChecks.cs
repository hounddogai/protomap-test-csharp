namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PendingPayBandQuotaChecks
{
    public static void PendingPayBandQuotaStartsInRequested()
    {
        var item = PendingPayBandQuota.NewPendingPayBandQuota("code-74", "name-401");
        Check.Equal(item.Status, PendingPayBandQuotaStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPayBandQuotaMovesToRevoked()
    {
        var item = PendingPayBandQuota.NewPendingPayBandQuota("code-74", "name-401");
        try
        {
            item.MoveTo(PendingPayBandQuotaStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPayBandQuotaStatus.Revoked, "status");
    }

    public static void PendingPayBandQuotaRejectsMovesFromExpired()
    {
        var item = PendingPayBandQuota.NewPendingPayBandQuota("code-74", "name-401");
        item.Status = PendingPayBandQuotaStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingPayBandQuotaStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingPayBandQuotaStartsInRequested();
        PendingPayBandQuotaMovesToRevoked();
        PendingPayBandQuotaRejectsMovesFromExpired();
    }
}
