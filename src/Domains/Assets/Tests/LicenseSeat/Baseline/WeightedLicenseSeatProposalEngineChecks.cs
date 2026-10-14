namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class WeightedLicenseSeatProposalEngineChecks
{
    public static void WeightedLicenseSeatProposalEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedLicenseSeatProposalEngine.DefaultWeightedLicenseSeatProposalEngine(Flags.NewRegistry());
        var item = WeightedLicenseSeatProposal.NewWeightedLicenseSeatProposal("code-782", "name-601");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedLicenseSeatProposalEngineKeepsResultsWithinBounds();
    }
}
