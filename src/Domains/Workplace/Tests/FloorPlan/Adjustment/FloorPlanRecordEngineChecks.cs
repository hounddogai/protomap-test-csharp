namespace Acme.Workforce.Workplace.Tests.FloorPlan.Adjustment;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class FloorPlanRecordEngineChecks
{
    public static void FloorPlanRecordEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanRecordEngine.DefaultFloorPlanRecordEngine();
        var item = FloorPlanRecord.NewFloorPlanRecord("code-632", "name-522");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanRecordEngineKeepsResultsWithinBounds();
    }
}
