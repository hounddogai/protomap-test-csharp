namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PrimaryMobilityProposalEngineChecks
{
    public static void PrimaryMobilityProposalEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryMobilityProposalEngine.DefaultPrimaryMobilityProposalEngine(Flags.NewRegistry());
        var item = PrimaryMobilityProposal.NewPrimaryMobilityProposal("code-902", "name-684");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryMobilityProposalEngineKeepsResultsWithinBounds();
    }
}
