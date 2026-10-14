namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class StrictPayBandQuotaViewChecks
{
    public static void StrictPayBandQuotaViewShowsTheStatusInCapitals()
    {
        var item = StrictPayBandQuota.NewStrictPayBandQuota("code-317", "label-505");
        var view = StrictPayBandQuotaView.ToStrictPayBandQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictPayBandQuotaView.ToStrictPayBandQuotaViews(new List<StrictPayBandQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictPayBandQuotaViewShowsTheStatusInCapitals();
    }
}
