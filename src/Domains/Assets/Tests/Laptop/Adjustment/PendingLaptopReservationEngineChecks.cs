namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingLaptopReservationEngineChecks
{
    public static void PendingLaptopReservationEngineKeepsResultsWithinBounds()
    {
        var engine = PendingLaptopReservationEngine.DefaultPendingLaptopReservationEngine(Flags.NewRegistry());
        var item = PendingLaptopReservation.NewPendingLaptopReservation("code-199", "title-197");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingLaptopReservationEngineKeepsResultsWithinBounds();
    }
}
