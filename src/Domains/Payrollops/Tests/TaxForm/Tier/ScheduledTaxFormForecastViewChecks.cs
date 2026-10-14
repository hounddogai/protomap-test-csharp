namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScheduledTaxFormForecastViewChecks
{
    public static void ScheduledTaxFormForecastViewShowsTheStatusInCapitals()
    {
        var item = ScheduledTaxFormForecast.NewScheduledTaxFormForecast("code-377", "title-691");
        var view = ScheduledTaxFormForecastView.ToScheduledTaxFormForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledTaxFormForecastView.ToScheduledTaxFormForecastViews(new List<ScheduledTaxFormForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledTaxFormForecastViewShowsTheStatusInCapitals();
    }
}
