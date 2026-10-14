namespace Acme.Workforce.Contractors.Tests.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientOffboardingForecastViewChecks
{
    public static void LenientOffboardingForecastViewShowsTheStatusInCapitals()
    {
        var item = LenientOffboardingForecast.NewLenientOffboardingForecast("code-67", "name-379");
        var view = LenientOffboardingForecastView.ToLenientOffboardingForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientOffboardingForecastView.ToLenientOffboardingForecastViews(new List<LenientOffboardingForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientOffboardingForecastViewShowsTheStatusInCapitals();
    }
}
