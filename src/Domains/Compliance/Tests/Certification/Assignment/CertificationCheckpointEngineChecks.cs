namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CertificationCheckpointEngineChecks
{
    public static void CertificationCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationCheckpointEngine.DefaultCertificationCheckpointEngine();
        var item = CertificationCheckpoint.NewCertificationCheckpoint("code-649", "label-404");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationCheckpointEngineKeepsResultsWithinBounds();
    }
}
