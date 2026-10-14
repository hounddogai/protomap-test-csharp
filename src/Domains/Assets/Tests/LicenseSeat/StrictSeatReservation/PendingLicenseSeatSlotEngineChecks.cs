namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class PendingLicenseSeatSlotEngineChecks
{
    public static void PendingLicenseSeatSlotEngineKeepsResultsWithinBounds()
    {
        var engine = PendingLicenseSeatSlotEngine.DefaultPendingLicenseSeatSlotEngine();
        var item = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot("code-752", "label-510");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingLicenseSeatSlotEngineKeepsResultsWithinBounds();
    }
}
