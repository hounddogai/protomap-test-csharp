namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class StrictAssignmentCheckpointEngineChecks
{
    public static void StrictAssignmentCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAssignmentCheckpointEngine.DefaultStrictAssignmentCheckpointEngine();
        var item = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint("code-8", "label-526");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAssignmentCheckpointEngineKeepsResultsWithinBounds();
    }
}
