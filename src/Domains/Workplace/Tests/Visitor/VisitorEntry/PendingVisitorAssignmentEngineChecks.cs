namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PendingVisitorAssignmentEngineChecks
{
    public static void PendingVisitorAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = PendingVisitorAssignmentEngine.DefaultPendingVisitorAssignmentEngine();
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("code-185", "label-951");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingVisitorAssignmentEngineKeepsResultsWithinBounds();
    }
}
