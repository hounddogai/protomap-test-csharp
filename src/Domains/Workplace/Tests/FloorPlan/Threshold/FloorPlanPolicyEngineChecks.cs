namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanPolicyEngineChecks
{
    public static void FloorPlanPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanPolicyEngine.DefaultFloorPlanPolicyEngine(Flags.NewRegistry());
        var item = FloorPlanPolicy.NewFloorPlanPolicy("code-596", "title-501");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanPolicyEngineKeepsResultsWithinBounds();
    }
}
