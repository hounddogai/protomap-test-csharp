namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PendingRefreshQuotaChecks
{
    public static void PendingRefreshQuotaStartsInRequested()
    {
        var item = PendingRefreshQuota.NewPendingRefreshQuota("code-634", "title-884");
        Check.Equal(item.Status, PendingRefreshQuotaStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingRefreshQuotaMovesToRevoked()
    {
        var item = PendingRefreshQuota.NewPendingRefreshQuota("code-634", "title-884");
        try
        {
            item.MoveTo(PendingRefreshQuotaStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingRefreshQuotaStatus.Revoked, "status");
    }

    public static void PendingRefreshQuotaRejectsMovesFromExpired()
    {
        var item = PendingRefreshQuota.NewPendingRefreshQuota("code-634", "title-884");
        item.Status = PendingRefreshQuotaStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingRefreshQuotaStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingRefreshQuotaStartsInRequested();
        PendingRefreshQuotaMovesToRevoked();
        PendingRefreshQuotaRejectsMovesFromExpired();
    }
}
