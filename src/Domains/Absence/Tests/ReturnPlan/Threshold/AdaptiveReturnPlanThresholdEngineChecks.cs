namespace Acme.Workforce.Absence.Tests.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanThresholdEngineChecks
{
    public static void AdaptiveReturnPlanThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveReturnPlanThresholdEngine.DefaultAdaptiveReturnPlanThresholdEngine();
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("code-509", "label-991");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveReturnPlanThresholdEngineKeepsResultsWithinBounds();
    }
}
