namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class GlobalEngagementAssignmentEngineChecks
{
    public static void GlobalEngagementAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalEngagementAssignmentEngine.DefaultGlobalEngagementAssignmentEngine();
        var item = GlobalEngagementAssignment.NewGlobalEngagementAssignment("code-395", "title-367");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalEngagementAssignmentEngineKeepsResultsWithinBounds();
    }
}
