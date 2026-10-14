namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class ScheduledBookingAuditEngineChecks
{
    public static void ScheduledBookingAuditEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledBookingAuditEngine.DefaultScheduledBookingAuditEngine();
        var item = ScheduledBookingAudit.NewScheduledBookingAudit("code-692", "title-203");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledBookingAuditEngineKeepsResultsWithinBounds();
    }
}
