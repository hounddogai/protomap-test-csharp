namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackAdjustmentProposalEngineChecks
{
    public static void FallbackAdjustmentProposalEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackAdjustmentProposalEngine.DefaultFallbackAdjustmentProposalEngine();
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("code-487", "label-265");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackAdjustmentProposalEngineKeepsResultsWithinBounds();
    }
}
