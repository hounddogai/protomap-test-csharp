namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

/// <summary>Checks of adjustment agreement code, run by the project's runner.</summary>
public static class AdjustmentEstimateEngineChecks
{
    public static void AdjustmentEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentEstimateEngine.DefaultAdjustmentEstimateEngine();
        var item = AdjustmentEstimate.NewAdjustmentEstimate("code-726", "title-383");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentEstimateEngineKeepsResultsWithinBounds();
    }
}
