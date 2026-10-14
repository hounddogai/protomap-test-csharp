namespace Acme.Workforce.Assets.Tests.Assignment.Tier;

using Acme.Workforce.Assets.Assignment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AssignmentSnapshotEngineChecks
{
    public static void AssignmentSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentSnapshotEngine.DefaultAssignmentSnapshotEngine();
        var item = AssignmentSnapshot.NewAssignmentSnapshot("code-559", "name-697");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentSnapshotEngineKeepsResultsWithinBounds();
    }
}
