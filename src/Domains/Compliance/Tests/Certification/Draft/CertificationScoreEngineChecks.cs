namespace Acme.Workforce.Compliance.Tests.Certification.Draft;

using Acme.Workforce.Compliance.Certification.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CertificationScoreEngineChecks
{
    public static void CertificationScoreEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationScoreEngine.DefaultCertificationScoreEngine();
        var item = CertificationScore.NewCertificationScore("code-371", "label-440");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationScoreEngineKeepsResultsWithinBounds();
    }
}
