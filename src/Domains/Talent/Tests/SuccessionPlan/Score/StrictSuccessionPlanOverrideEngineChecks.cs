namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictSuccessionPlanOverrideEngineChecks
{
    public static void StrictSuccessionPlanOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = StrictSuccessionPlanOverrideEngine.DefaultStrictSuccessionPlanOverrideEngine(Flags.NewRegistry());
        var item = StrictSuccessionPlanOverride.NewStrictSuccessionPlanOverride("code-256", "title-868");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictSuccessionPlanOverrideEngineKeepsResultsWithinBounds();
    }
}
