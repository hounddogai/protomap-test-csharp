namespace Acme.Workforce.Assets.Tests.Assignment.Estimate;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AssignmentMetricEngineChecks
{
    public static void AssignmentMetricEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentMetricEngine.DefaultAssignmentMetricEngine();
        var item = AssignmentMetric.NewAssignmentMetric("code-562", "label-556");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentMetricEngineKeepsResultsWithinBounds();
    }
}
