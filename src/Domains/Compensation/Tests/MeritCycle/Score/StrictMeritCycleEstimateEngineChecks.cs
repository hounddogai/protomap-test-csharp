namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictMeritCycleEstimateEngineChecks
{
    public static void StrictMeritCycleEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = StrictMeritCycleEstimateEngine.DefaultStrictMeritCycleEstimateEngine(Flags.NewRegistry());
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("code-577", "label-745");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictMeritCycleEstimateEngineKeepsResultsWithinBounds();
    }
}
