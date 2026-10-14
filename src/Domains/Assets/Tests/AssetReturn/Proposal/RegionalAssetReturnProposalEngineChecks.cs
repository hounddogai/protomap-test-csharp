namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RegionalAssetReturnProposalEngineChecks
{
    public static void RegionalAssetReturnProposalEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalAssetReturnProposalEngine.DefaultRegionalAssetReturnProposalEngine(Flags.NewRegistry());
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("code-936", "title-394");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalAssetReturnProposalEngineKeepsResultsWithinBounds();
    }
}
