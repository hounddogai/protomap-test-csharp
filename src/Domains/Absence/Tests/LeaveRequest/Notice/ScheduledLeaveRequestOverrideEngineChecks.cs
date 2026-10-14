namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestOverrideEngineChecks
{
    public static void ScheduledLeaveRequestOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledLeaveRequestOverrideEngine.DefaultScheduledLeaveRequestOverrideEngine(Flags.NewRegistry());
        var item = ScheduledLeaveRequestOverride.NewScheduledLeaveRequestOverride("code-968", "name-379");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestOverrideEngineKeepsResultsWithinBounds();
    }
}
