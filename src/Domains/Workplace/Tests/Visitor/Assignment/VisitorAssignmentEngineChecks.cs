namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorAssignmentEngineChecks
{
    public static void VisitorAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorAssignmentEngine.DefaultVisitorAssignmentEngine();
        var item = VisitorAssignment.NewVisitorAssignment("code-608", "title-504");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorAssignmentEngineKeepsResultsWithinBounds();
    }
}
