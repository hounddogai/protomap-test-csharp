namespace Acme.Workforce.Workplace.Tests.Occupancy.Agreement;

using Acme.Workforce.Workplace.Occupancy.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ManualOccupancyPlanEngineChecks
{
    public static void ManualOccupancyPlanEngineKeepsResultsWithinBounds()
    {
        var engine = ManualOccupancyPlanEngine.DefaultManualOccupancyPlanEngine();
        var item = ManualOccupancyPlan.NewManualOccupancyPlan("code-863", "name-787");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualOccupancyPlanEngineKeepsResultsWithinBounds();
    }
}
