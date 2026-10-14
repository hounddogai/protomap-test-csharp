namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ManualLeaveRequestReservationEngineChecks
{
    public static void ManualLeaveRequestReservationEngineKeepsResultsWithinBounds()
    {
        var engine = ManualLeaveRequestReservationEngine.DefaultManualLeaveRequestReservationEngine(Flags.NewRegistry());
        var item = ManualLeaveRequestReservation.NewManualLeaveRequestReservation("code-116", "title-717");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualLeaveRequestReservationEngineKeepsResultsWithinBounds();
    }
}
