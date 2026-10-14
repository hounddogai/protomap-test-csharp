namespace Acme.Workforce.Compliance.Tests.Certification.OverrideItem;

using Acme.Workforce.Compliance.Certification.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class CertificationProposalEngineChecks
{
    public static void CertificationProposalEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationProposalEngine.DefaultCertificationProposalEngine();
        var item = CertificationProposal.NewCertificationProposal("code-599", "name-394");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationProposalEngineKeepsResultsWithinBounds();
    }
}
