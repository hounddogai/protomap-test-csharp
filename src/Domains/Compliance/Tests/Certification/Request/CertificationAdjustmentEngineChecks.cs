namespace Acme.Workforce.Compliance.Tests.Certification.Request;

using Acme.Workforce.Compliance.Certification.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class CertificationAdjustmentEngineChecks
{
    public static void CertificationAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationAdjustmentEngine.DefaultCertificationAdjustmentEngine();
        var item = CertificationAdjustment.NewCertificationAdjustment("code-331", "label-30");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationAdjustmentEngineKeepsResultsWithinBounds();
    }
}
