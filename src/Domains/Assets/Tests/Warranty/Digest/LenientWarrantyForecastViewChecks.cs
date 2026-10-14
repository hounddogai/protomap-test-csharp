namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LenientWarrantyForecastViewChecks
{
    public static void LenientWarrantyForecastViewShowsTheStatusInCapitals()
    {
        var item = LenientWarrantyForecast.NewLenientWarrantyForecast("code-675", "label-486");
        var view = LenientWarrantyForecastView.ToLenientWarrantyForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientWarrantyForecastView.ToLenientWarrantyForecastViews(new List<LenientWarrantyForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientWarrantyForecastViewShowsTheStatusInCapitals();
    }
}
