namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalExceptionSnapshotEngineChecks
{
    public static void GlobalExceptionSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalExceptionSnapshotEngine.DefaultGlobalExceptionSnapshotEngine();
        var item = GlobalExceptionSnapshot.NewGlobalExceptionSnapshot("code-513", "title-154");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalExceptionSnapshotEngineKeepsResultsWithinBounds();
    }
}
