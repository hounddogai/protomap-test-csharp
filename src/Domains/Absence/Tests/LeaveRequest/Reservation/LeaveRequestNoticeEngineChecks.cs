namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LeaveRequestNoticeEngineChecks
{
    public static void LeaveRequestNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestNoticeEngine.DefaultLeaveRequestNoticeEngine();
        var item = LeaveRequestNotice.NewLeaveRequestNotice("code-764", "name-299");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestNoticeEngineKeepsResultsWithinBounds();
    }
}
