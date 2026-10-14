namespace Acme.Workforce.Assets.Tests.Warranty.Reservation;

using Acme.Workforce.Assets.Warranty.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WarrantyReservationEngineChecks
{
    public static void WarrantyReservationEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyReservationEngine.DefaultWarrantyReservationEngine();
        var item = WarrantyReservation.NewWarrantyReservation("code-254", "title-140");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyReservationEngineKeepsResultsWithinBounds();
    }
}
