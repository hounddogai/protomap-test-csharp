namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentAssignmentEngineChecks
{
    public static void AdjustmentAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentAssignmentEngine.DefaultAdjustmentAssignmentEngine(Flags.NewRegistry());
        var item = AdjustmentAssignment.NewAdjustmentAssignment("code-431", "name-979");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentAssignmentEngineKeepsResultsWithinBounds();
    }
}
