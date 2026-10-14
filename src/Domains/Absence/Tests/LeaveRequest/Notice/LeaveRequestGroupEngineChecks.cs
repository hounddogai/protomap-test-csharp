namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestGroupEngineChecks
{
    public static void LeaveRequestGroupEngineKeepsResultsWithinBounds()
    {
        var engine = LeaveRequestGroupEngine.DefaultLeaveRequestGroupEngine(Flags.NewRegistry());
        var item = LeaveRequestGroup.NewLeaveRequestGroup("code-31", "name-777");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LeaveRequestGroupEngineKeepsResultsWithinBounds();
    }
}
