namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientLeaveRequestReservationEngineChecks
{
    public static void LenientLeaveRequestReservationEngineKeepsResultsWithinBounds()
    {
        var engine = LenientLeaveRequestReservationEngine.DefaultLenientLeaveRequestReservationEngine();
        var item = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation("code-456", "title-612");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientLeaveRequestReservationEngineKeepsResultsWithinBounds();
    }
}
