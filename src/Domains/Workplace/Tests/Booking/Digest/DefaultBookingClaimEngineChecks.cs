namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DefaultBookingClaimEngineChecks
{
    public static void DefaultBookingClaimEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultBookingClaimEngine.DefaultDefaultBookingClaimEngine();
        var item = DefaultBookingClaim.NewDefaultBookingClaim("code-592", "name-278");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultBookingClaimEngineKeepsResultsWithinBounds();
    }
}
