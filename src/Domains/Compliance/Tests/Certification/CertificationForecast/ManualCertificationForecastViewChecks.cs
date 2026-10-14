namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class ManualCertificationForecastViewChecks
{
    public static void ManualCertificationForecastViewShowsTheStatusInCapitals()
    {
        var item = ManualCertificationForecast.NewManualCertificationForecast("code-125", "name-499");
        var view = ManualCertificationForecastView.ToManualCertificationForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualCertificationForecastView.ToManualCertificationForecastViews(new List<ManualCertificationForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualCertificationForecastViewShowsTheStatusInCapitals();
    }
}
