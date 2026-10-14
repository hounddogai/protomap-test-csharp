namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandForecastViewChecks
{
    public static void PayBandForecastViewShowsTheStatusInCapitals()
    {
        var item = PayBandForecast.NewPayBandForecast("code-348", "label-555");
        var view = PayBandForecastView.ToPayBandForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandForecastView.ToPayBandForecastViews(new List<PayBandForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandForecastViewShowsTheStatusInCapitals();
    }
}
