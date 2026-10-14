namespace Acme.Workforce.Absence.Tests.LeaveRequest.Budget;

using Acme.Workforce.Absence.LeaveRequest.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LeaveRequestBatchEngineChecks
{
    public static void LeaveRequestBatchEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestBatchEngine.DefaultLeaveRequestBatchEngine();
        var item = LeaveRequestBatch.NewLeaveRequestBatch("code-117", "name-107");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestBatchEngineKeepsResultsWithinBounds();
    }
}
