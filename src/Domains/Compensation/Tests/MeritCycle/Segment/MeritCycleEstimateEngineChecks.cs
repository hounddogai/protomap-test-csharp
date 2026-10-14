namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class MeritCycleEstimateEngineChecks
{
    public static void MeritCycleEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleEstimateEngine.DefaultMeritCycleEstimateEngine(Flags.NewRegistry());
        var item = MeritCycleEstimate.NewMeritCycleEstimate("code-789", "label-453");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleEstimateEngineKeepsResultsWithinBounds();
    }
}
