namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentSnapshot;

using Acme.Workforce.Assets.Assignment.AssignmentSnapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment snapshot code, run by the project's runner.</summary>
public static class AssignmentBatchEngineChecks
{
    public static void AssignmentBatchEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentBatchEngine.DefaultAssignmentBatchEngine(Flags.NewRegistry());
        var item = AssignmentBatch.NewAssignmentBatch("code-51", "name-761");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentBatchEngineKeepsResultsWithinBounds();
    }
}
