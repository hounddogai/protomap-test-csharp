namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StrictPoolPlanEngineChecks
{
    public static void StrictPoolPlanEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPoolPlanEngine.DefaultStrictPoolPlanEngine(Flags.NewRegistry());
        var item = StrictPoolPlan.NewStrictPoolPlan("code-451", "label-857");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPoolPlanEngineKeepsResultsWithinBounds();
    }
}
