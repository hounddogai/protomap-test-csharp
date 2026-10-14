namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalAssignmentProposalEngineChecks
{
    public static void SeasonalAssignmentProposalEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalAssignmentProposalEngine.DefaultSeasonalAssignmentProposalEngine(Flags.NewRegistry());
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("code-808", "name-108");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalAssignmentProposalEngineKeepsResultsWithinBounds();
    }
}
