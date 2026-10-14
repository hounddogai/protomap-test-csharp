namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanThresholdEngineChecks
{
    public static void SuccessionPlanThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanThresholdEngine.DefaultSuccessionPlanThresholdEngine();
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("code-729", "title-269");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanThresholdEngineKeepsResultsWithinBounds();
    }
}
