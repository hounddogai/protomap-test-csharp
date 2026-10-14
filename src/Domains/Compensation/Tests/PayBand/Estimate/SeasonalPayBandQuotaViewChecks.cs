namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalPayBandQuotaViewChecks
{
    public static void SeasonalPayBandQuotaViewShowsTheStatusInCapitals()
    {
        var item = SeasonalPayBandQuota.NewSeasonalPayBandQuota("code-589", "label-24");
        var view = SeasonalPayBandQuotaView.ToSeasonalPayBandQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalPayBandQuotaView.ToSeasonalPayBandQuotaViews(new List<SeasonalPayBandQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalPayBandQuotaViewShowsTheStatusInCapitals();
    }
}
