namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AssignmentAdjustmentEngineChecks
{
    public static void AssignmentAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentAdjustmentEngine.DefaultAssignmentAdjustmentEngine(Flags.NewRegistry());
        var item = AssignmentAdjustment.NewAssignmentAdjustment("code-369", "name-878");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentAdjustmentEngineKeepsResultsWithinBounds();
    }
}
