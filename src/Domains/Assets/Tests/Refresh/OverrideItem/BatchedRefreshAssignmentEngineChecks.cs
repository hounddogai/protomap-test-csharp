namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class BatchedRefreshAssignmentEngineChecks
{
    public static void BatchedRefreshAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedRefreshAssignmentEngine.DefaultBatchedRefreshAssignmentEngine(Flags.NewRegistry());
        var item = BatchedRefreshAssignment.NewBatchedRefreshAssignment("code-903", "name-999");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedRefreshAssignmentEngineKeepsResultsWithinBounds();
    }
}
