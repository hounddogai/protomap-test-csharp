namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestRollupEngineChecks
{
    public static void ScheduledLeaveRequestRollupEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledLeaveRequestRollupEngine.DefaultScheduledLeaveRequestRollupEngine();
        var item = ScheduledLeaveRequestRollup.NewScheduledLeaveRequestRollup("code-521", "label-500");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestRollupEngineKeepsResultsWithinBounds();
    }
}
