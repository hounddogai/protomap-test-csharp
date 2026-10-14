namespace Acme.Workforce.Compliance.Tests.Certification.Summary;

using Acme.Workforce.Compliance.Certification.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CertificationSummaryEngineChecks
{
    public static void CertificationSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationSummaryEngine.DefaultCertificationSummaryEngine(Flags.NewRegistry());
        var item = CertificationSummary.NewCertificationSummary("code-1", "label-362");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationSummaryEngineKeepsResultsWithinBounds();
    }
}
