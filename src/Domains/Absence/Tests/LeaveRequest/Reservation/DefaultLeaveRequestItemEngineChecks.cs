namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultLeaveRequestItemEngineChecks
{
    public static void DefaultLeaveRequestItemEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultLeaveRequestItemEngine.DefaultDefaultLeaveRequestItemEngine(Flags.NewRegistry());
        var item = DefaultLeaveRequestItem.NewDefaultLeaveRequestItem("code-401", "title-451");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultLeaveRequestItemEngineKeepsResultsWithinBounds();
    }
}
