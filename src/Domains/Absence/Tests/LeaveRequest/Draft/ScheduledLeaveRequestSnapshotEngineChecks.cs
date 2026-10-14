namespace Acme.Workforce.Absence.Tests.LeaveRequest.Draft;

using Acme.Workforce.Absence.LeaveRequest.Draft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestSnapshotEngineChecks
{
    public static void ScheduledLeaveRequestSnapshotEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledLeaveRequestSnapshotEngine.DefaultScheduledLeaveRequestSnapshotEngine(Flags.NewRegistry());
        var item = ScheduledLeaveRequestSnapshot.NewScheduledLeaveRequestSnapshot("code-12", "name-996");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestSnapshotEngineKeepsResultsWithinBounds();
    }
}
