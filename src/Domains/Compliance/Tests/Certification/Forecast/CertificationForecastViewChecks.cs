namespace Acme.Workforce.Compliance.Tests.Certification.Forecast;

using Acme.Workforce.Compliance.Certification.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class CertificationForecastViewChecks
{
    public static void CertificationForecastViewShowsTheStatusInCapitals()
    {
        var item = CertificationForecast.NewCertificationForecast("code-421", "name-836");
        var view = CertificationForecastView.ToCertificationForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CertificationForecastView.ToCertificationForecastViews(new List<CertificationForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CertificationForecastViewShowsTheStatusInCapitals();
    }
}
