namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopForecastViewChecks
{
    public static void LaptopForecastViewShowsTheStatusInCapitals()
    {
        var item = LaptopForecast.NewLaptopForecast("code-812", "title-523");
        var view = LaptopForecastView.ToLaptopForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopForecastView.ToLaptopForecastViews(new List<LaptopForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopForecastViewShowsTheStatusInCapitals();
    }
}
