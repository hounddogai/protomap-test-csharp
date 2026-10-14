namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class MobilityProposalEngineChecks
{
    public static void MobilityProposalEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityProposalEngine.DefaultMobilityProposalEngine();
        var item = MobilityProposal.NewMobilityProposal("code-96", "name-493");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityProposalEngineKeepsResultsWithinBounds();
    }
}
