namespace Acme.Workforce.Workplace.Tests.Booking.StrictBookingPlan;

using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Checks of strict booking plan code, run by the project's runner.</summary>
public static class BookingSlotEngineChecks
{
    public static void BookingSlotEngineKeepsResultsWithinBounds()
    {
        var engine = BookingSlotEngine.DefaultBookingSlotEngine();
        var item = BookingSlot.NewBookingSlot("code-704", "title-947");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingSlotEngineKeepsResultsWithinBounds();
    }
}
