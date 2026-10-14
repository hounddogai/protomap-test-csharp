namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

/// <summary>Checks of occupancy plan code, run by the project's runner.</summary>
public static class OccupancyBudgetEngineChecks
{
    public static void OccupancyBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyBudgetEngine.DefaultOccupancyBudgetEngine(Flags.NewRegistry());
        var item = OccupancyBudget.NewOccupancyBudget("code-237", "label-556");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyBudgetEngineKeepsResultsWithinBounds();
    }
}
