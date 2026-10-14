namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PendingBookingDigestEngineChecks
{
    public static void PendingBookingDigestEngineKeepsResultsWithinBounds()
    {
        var engine = PendingBookingDigestEngine.DefaultPendingBookingDigestEngine();
        var item = PendingBookingDigest.NewPendingBookingDigest("code-412", "label-174");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingBookingDigestEngineKeepsResultsWithinBounds();
    }
}
