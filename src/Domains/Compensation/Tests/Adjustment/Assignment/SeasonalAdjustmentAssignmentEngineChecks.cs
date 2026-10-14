namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class SeasonalAdjustmentAssignmentEngineChecks
{
    public static void SeasonalAdjustmentAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalAdjustmentAssignmentEngine.DefaultSeasonalAdjustmentAssignmentEngine(Flags.NewRegistry());
        var item = SeasonalAdjustmentAssignment.NewSeasonalAdjustmentAssignment("code-66", "title-876");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentAssignmentEngineKeepsResultsWithinBounds();
    }
}
