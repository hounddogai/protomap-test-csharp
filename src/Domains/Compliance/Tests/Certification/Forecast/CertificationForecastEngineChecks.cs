namespace Acme.Workforce.Compliance.Tests.Certification.Forecast;

using Acme.Workforce.Compliance.Certification.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class CertificationForecastEngineChecks
{
    public static void CertificationForecastEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationForecastEngine.DefaultCertificationForecastEngine();
        var item = CertificationForecast.NewCertificationForecast("code-421", "name-836");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationForecastEngineKeepsResultsWithinBounds();
    }
}
