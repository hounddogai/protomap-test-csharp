namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class CertificationThresholdEngineChecks
{
    public static void CertificationThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationThresholdEngine.DefaultCertificationThresholdEngine();
        var item = CertificationThreshold.NewCertificationThreshold("code-699", "title-493");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationThresholdEngineKeepsResultsWithinBounds();
    }
}
