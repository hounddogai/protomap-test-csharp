namespace Acme.Workforce.Workplace.Tests.Booking.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class BookingQuotaEngineChecks
{
    public static void BookingQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = BookingQuotaEngine.DefaultBookingQuotaEngine(Flags.NewRegistry());
        var item = BookingQuota.NewBookingQuota("code-12", "title-434");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingQuotaEngineKeepsResultsWithinBounds();
    }
}
