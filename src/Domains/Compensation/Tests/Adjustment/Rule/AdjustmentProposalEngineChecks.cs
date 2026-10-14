namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentProposalEngineChecks
{
    public static void AdjustmentProposalEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentProposalEngine.DefaultAdjustmentProposalEngine();
        var item = AdjustmentProposal.NewAdjustmentProposal("code-369", "name-463");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentProposalEngineKeepsResultsWithinBounds();
    }
}
