namespace Acme.Workforce.Absence.Tests.LeaveRequest.Digest;

using Acme.Workforce.Absence.LeaveRequest.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LeaveRequestPlanEngineChecks
{
    public static void LeaveRequestPlanEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestPlanEngine.DefaultLeaveRequestPlanEngine();
        var item = LeaveRequestPlan.NewLeaveRequestPlan("code-611", "name-401");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestPlanEngineKeepsResultsWithinBounds();
    }
}
