namespace Acme.Workforce.Engagement.Tests.ActionPlan.Draft;

using Acme.Workforce.Engagement.ActionPlan.Draft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PrimaryActionPlanMappingEngineChecks
{
    public static void PrimaryActionPlanMappingEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryActionPlanMappingEngine.DefaultPrimaryActionPlanMappingEngine(Flags.NewRegistry());
        var item = PrimaryActionPlanMapping.NewPrimaryActionPlanMapping("code-234", "title-997");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryActionPlanMappingEngineKeepsResultsWithinBounds();
    }
}
