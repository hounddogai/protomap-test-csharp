namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class LegacyBookingAuditEngineChecks
{
    public static void LegacyBookingAuditEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyBookingAuditEngine.DefaultLegacyBookingAuditEngine(Flags.NewRegistry());
        var item = LegacyBookingAudit.NewLegacyBookingAudit("code-847", "title-109");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyBookingAuditEngineKeepsResultsWithinBounds();
    }
}
