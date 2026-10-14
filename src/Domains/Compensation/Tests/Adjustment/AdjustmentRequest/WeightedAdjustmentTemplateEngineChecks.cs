namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class WeightedAdjustmentTemplateEngineChecks
{
    public static void WeightedAdjustmentTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAdjustmentTemplateEngine.DefaultWeightedAdjustmentTemplateEngine(Flags.NewRegistry());
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("code-4", "label-838");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAdjustmentTemplateEngineKeepsResultsWithinBounds();
    }
}
