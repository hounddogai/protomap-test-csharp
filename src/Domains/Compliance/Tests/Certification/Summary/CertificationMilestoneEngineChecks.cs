namespace Acme.Workforce.Compliance.Tests.Certification.Summary;

using Acme.Workforce.Compliance.Certification.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class CertificationMilestoneEngineChecks
{
    public static void CertificationMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationMilestoneEngine.DefaultCertificationMilestoneEngine(Flags.NewRegistry());
        var item = CertificationMilestone.NewCertificationMilestone("code-557", "title-371");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationMilestoneEngineKeepsResultsWithinBounds();
    }
}
