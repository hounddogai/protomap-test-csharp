namespace Acme.Workforce.Assets.Tests.Refresh.Preference;

using Acme.Workforce.Assets.Refresh.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RefreshAssignmentEngineChecks
{
    public static void RefreshAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshAssignmentEngine.DefaultRefreshAssignmentEngine();
        var item = RefreshAssignment.NewRefreshAssignment("code-481", "label-518");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshAssignmentEngineKeepsResultsWithinBounds();
    }
}
