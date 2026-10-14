namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestNoticeEngineChecks
{
    public static void ScheduledLeaveRequestNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledLeaveRequestNoticeEngine.DefaultScheduledLeaveRequestNoticeEngine(Flags.NewRegistry());
        var item = ScheduledLeaveRequestNotice.NewScheduledLeaveRequestNotice("code-240", "name-628");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestNoticeEngineKeepsResultsWithinBounds();
    }
}
