namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BookingAssignmentEngineChecks
{
    public static void BookingAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = BookingAssignmentEngine.DefaultBookingAssignmentEngine(Flags.NewRegistry());
        var item = BookingAssignment.NewBookingAssignment("code-359", "name-538");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingAssignmentEngineKeepsResultsWithinBounds();
    }
}
