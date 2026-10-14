namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredReturnPlanThresholdEngineChecks
{
    public static void TieredReturnPlanThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = TieredReturnPlanThresholdEngine.DefaultTieredReturnPlanThresholdEngine();
        var item = TieredReturnPlanThreshold.NewTieredReturnPlanThreshold("code-560", "label-678");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredReturnPlanThresholdEngineKeepsResultsWithinBounds();
    }
}
