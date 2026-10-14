namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SuccessionPlanOverrideEngineChecks
{
    public static void SuccessionPlanOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanOverrideEngine.DefaultSuccessionPlanOverrideEngine(Flags.NewRegistry());
        var item = SuccessionPlanOverride.NewSuccessionPlanOverride("code-926", "name-300");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanOverrideEngineKeepsResultsWithinBounds();
    }
}
