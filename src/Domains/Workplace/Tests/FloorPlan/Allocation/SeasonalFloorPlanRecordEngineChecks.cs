namespace Acme.Workforce.Workplace.Tests.FloorPlan.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class SeasonalFloorPlanRecordEngineChecks
{
    public static void SeasonalFloorPlanRecordEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalFloorPlanRecordEngine.DefaultSeasonalFloorPlanRecordEngine(Flags.NewRegistry());
        var item = SeasonalFloorPlanRecord.NewSeasonalFloorPlanRecord("code-146", "label-742");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalFloorPlanRecordEngineKeepsResultsWithinBounds();
    }
}
