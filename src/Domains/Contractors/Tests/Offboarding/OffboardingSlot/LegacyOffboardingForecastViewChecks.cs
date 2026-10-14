namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSlot;

using Acme.Workforce.Contractors.Offboarding.OffboardingSlot;

/// <summary>Checks of offboarding slot code, run by the project's runner.</summary>
public static class LegacyOffboardingForecastViewChecks
{
    public static void LegacyOffboardingForecastViewShowsTheStatusInCapitals()
    {
        var item = LegacyOffboardingForecast.NewLegacyOffboardingForecast("code-440", "label-686");
        var view = LegacyOffboardingForecastView.ToLegacyOffboardingForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyOffboardingForecastView.ToLegacyOffboardingForecastViews(new List<LegacyOffboardingForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyOffboardingForecastViewShowsTheStatusInCapitals();
    }
}
