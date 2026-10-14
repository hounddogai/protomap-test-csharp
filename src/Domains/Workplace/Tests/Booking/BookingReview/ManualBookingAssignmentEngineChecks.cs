namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class ManualBookingAssignmentEngineChecks
{
    public static void ManualBookingAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = ManualBookingAssignmentEngine.DefaultManualBookingAssignmentEngine(Flags.NewRegistry());
        var item = ManualBookingAssignment.NewManualBookingAssignment("code-835", "name-592");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualBookingAssignmentEngineKeepsResultsWithinBounds();
    }
}
