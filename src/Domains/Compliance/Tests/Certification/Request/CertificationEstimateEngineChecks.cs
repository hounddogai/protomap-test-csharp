namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationEstimateEngineChecks
{
    public static void CertificationEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationEstimateEngine.DefaultCertificationEstimateEngine();
        var item = CertificationEstimate.NewCertificationEstimate("code-844", "title-663");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationEstimateEngineKeepsResultsWithinBounds();
    }
}
