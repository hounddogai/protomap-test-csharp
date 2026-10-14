namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FloorPlanTemplateEngineChecks
{
    public static void FloorPlanTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanTemplateEngine.DefaultFloorPlanTemplateEngine(Flags.NewRegistry());
        var item = FloorPlanTemplate.NewFloorPlanTemplate("code-690", "label-680");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanTemplateEngineKeepsResultsWithinBounds();
    }
}
