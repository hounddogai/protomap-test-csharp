namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class WeightedMeritCycleTierEngineChecks
{
    public static void WeightedMeritCycleTierEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedMeritCycleTierEngine.DefaultWeightedMeritCycleTierEngine();
        var item = WeightedMeritCycleTier.NewWeightedMeritCycleTier("code-176", "label-675");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedMeritCycleTierEngineKeepsResultsWithinBounds();
    }
}
