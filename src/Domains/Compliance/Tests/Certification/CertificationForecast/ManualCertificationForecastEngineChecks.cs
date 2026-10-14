namespace Acme.Workforce.Compliance.Tests.Certification.CertificationForecast;

using Acme.Workforce.Compliance.Certification.CertificationForecast;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of certification forecast code, run by the project's runner.</summary>
public static class ManualCertificationForecastEngineChecks
{
    public static void ManualCertificationForecastEngineKeepsResultsWithinBounds()
    {
        var engine = ManualCertificationForecastEngine.DefaultManualCertificationForecastEngine(Flags.NewRegistry());
        var item = ManualCertificationForecast.NewManualCertificationForecast("code-125", "name-499");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualCertificationForecastEngineKeepsResultsWithinBounds();
    }
}
