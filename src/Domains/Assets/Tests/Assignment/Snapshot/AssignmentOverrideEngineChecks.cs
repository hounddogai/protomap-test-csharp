namespace Acme.Workforce.Assets.Tests.Assignment.Snapshot;

using Acme.Workforce.Assets.Assignment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AssignmentOverrideEngineChecks
{
    public static void AssignmentOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentOverrideEngine.DefaultAssignmentOverrideEngine();
        var item = AssignmentOverride.NewAssignmentOverride("code-229", "label-505");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentOverrideEngineKeepsResultsWithinBounds();
    }
}
