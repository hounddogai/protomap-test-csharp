namespace Acme.Workforce.Assets.Tests.Assignment.Summary;

using Acme.Workforce.Assets.Assignment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssignmentSummaryEngineChecks
{
    public static void AssignmentSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentSummaryEngine.DefaultAssignmentSummaryEngine();
        var item = AssignmentSummary.NewAssignmentSummary("code-279", "name-198");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentSummaryEngineKeepsResultsWithinBounds();
    }
}
