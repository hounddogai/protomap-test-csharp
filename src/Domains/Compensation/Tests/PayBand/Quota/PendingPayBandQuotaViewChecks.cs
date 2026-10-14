namespace Acme.Workforce.Compensation.Tests.PayBand.Quota;

using Acme.Workforce.Compensation.PayBand.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PendingPayBandQuotaViewChecks
{
    public static void PendingPayBandQuotaViewShowsTheStatusInCapitals()
    {
        var item = PendingPayBandQuota.NewPendingPayBandQuota("code-74", "name-401");
        var view = PendingPayBandQuotaView.ToPendingPayBandQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPayBandQuotaView.ToPendingPayBandQuotaViews(new List<PendingPayBandQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPayBandQuotaViewShowsTheStatusInCapitals();
    }
}
