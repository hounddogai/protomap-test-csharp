namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestBaselineEngineChecks
{
    public static void ScheduledLeaveRequestBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledLeaveRequestBaselineEngine.DefaultScheduledLeaveRequestBaselineEngine(Flags.NewRegistry());
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("code-100", "title-97");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestBaselineEngineKeepsResultsWithinBounds();
    }
}
