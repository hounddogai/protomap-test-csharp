namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class RegionalFloorPlanBaselineEngineChecks
{
    public static void RegionalFloorPlanBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalFloorPlanBaselineEngine.DefaultRegionalFloorPlanBaselineEngine(Flags.NewRegistry());
        var item = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline("code-906", "label-553");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalFloorPlanBaselineEngineKeepsResultsWithinBounds();
    }
}
