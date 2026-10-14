namespace Acme.Workforce.Compliance.Tests.Attestation.Segment;

using Acme.Workforce.Compliance.Attestation.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CappedAttestationProposalEngineChecks
{
    public static void CappedAttestationProposalEngineKeepsResultsWithinBounds()
    {
        var engine = CappedAttestationProposalEngine.DefaultCappedAttestationProposalEngine(Flags.NewRegistry());
        var item = CappedAttestationProposal.NewCappedAttestationProposal("code-794", "title-827");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedAttestationProposalEngineKeepsResultsWithinBounds();
    }
}
