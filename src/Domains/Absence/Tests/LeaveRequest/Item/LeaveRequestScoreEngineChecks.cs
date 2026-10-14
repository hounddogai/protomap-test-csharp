namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestScoreEngineChecks
{
    public static void LeaveRequestScoreEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestScoreEngine.DefaultLeaveRequestScoreEngine();
        var item = LeaveRequestScore.NewLeaveRequestScore("code-315", "title-787");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestScoreEngineKeepsResultsWithinBounds();
    }
}
