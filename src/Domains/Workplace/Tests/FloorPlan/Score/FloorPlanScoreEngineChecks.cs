namespace Acme.Workforce.Workplace.Tests.FloorPlan.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FloorPlanScoreEngineChecks
{
    public static void FloorPlanScoreEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanScoreEngine.DefaultFloorPlanScoreEngine(Flags.NewRegistry());
        var item = FloorPlanScore.NewFloorPlanScore("code-415", "name-513");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanScoreEngineKeepsResultsWithinBounds();
    }
}
