namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingAgreementEngineChecks
{
    public static void BookingAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = BookingAgreementEngine.DefaultBookingAgreementEngine(Flags.NewRegistry());
        var item = BookingAgreement.NewBookingAgreement("code-870", "name-657");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingAgreementEngineKeepsResultsWithinBounds();
    }
}
