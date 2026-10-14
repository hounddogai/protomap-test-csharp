namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PendingRefreshQuotaViewChecks
{
    public static void PendingRefreshQuotaViewShowsTheStatusInCapitals()
    {
        var item = PendingRefreshQuota.NewPendingRefreshQuota("code-634", "title-884");
        var view = PendingRefreshQuotaView.ToPendingRefreshQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingRefreshQuotaView.ToPendingRefreshQuotaViews(new List<PendingRefreshQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingRefreshQuotaViewShowsTheStatusInCapitals();
    }
}
