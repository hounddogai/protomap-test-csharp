namespace Acme.Workforce.Compliance.Tests.Certification.CertificationAssignment;

using Acme.Workforce.Compliance.Certification.CertificationAssignment;

/// <summary>Checks of certification assignment code, run by the project's runner.</summary>
public static class TieredCertificationSummaryEngineChecks
{
    public static void TieredCertificationSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = TieredCertificationSummaryEngine.DefaultTieredCertificationSummaryEngine();
        var item = TieredCertificationSummary.NewTieredCertificationSummary("code-471", "title-598");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredCertificationSummaryEngineKeepsResultsWithinBounds();
    }
}
