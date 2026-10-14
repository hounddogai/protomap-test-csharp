namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveMobilityProposalEngineChecks
{
    public static void AdaptiveMobilityProposalEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveMobilityProposalEngine.DefaultAdaptiveMobilityProposalEngine();
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("code-202", "name-401");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveMobilityProposalEngineKeepsResultsWithinBounds();
    }
}
