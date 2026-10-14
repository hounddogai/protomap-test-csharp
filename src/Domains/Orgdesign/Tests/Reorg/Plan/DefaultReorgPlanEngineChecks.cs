namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultReorgPlanEngineChecks
{
    public static void DefaultReorgPlanEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultReorgPlanEngine.DefaultDefaultReorgPlanEngine(Flags.NewRegistry());
        var item = DefaultReorgPlan.NewDefaultReorgPlan("code-358", "title-158");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultReorgPlanEngineKeepsResultsWithinBounds();
    }
}
