namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BookingRecordEngineChecks
{
    public static void BookingRecordEngineKeepsResultsWithinBounds()
    {
        var engine = BookingRecordEngine.DefaultBookingRecordEngine();
        var item = BookingRecord.NewBookingRecord("code-344", "label-759");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingRecordEngineKeepsResultsWithinBounds();
    }
}
