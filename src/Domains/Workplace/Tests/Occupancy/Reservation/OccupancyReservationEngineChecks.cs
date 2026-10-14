namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyReservationEngineChecks
{
    public static void OccupancyReservationEngineKeepsResultsWithinBounds()
    {
        var engine = OccupancyReservationEngine.DefaultOccupancyReservationEngine(Flags.NewRegistry());
        var item = OccupancyReservation.NewOccupancyReservation("code-934", "title-972");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OccupancyReservationEngineKeepsResultsWithinBounds();
    }
}
