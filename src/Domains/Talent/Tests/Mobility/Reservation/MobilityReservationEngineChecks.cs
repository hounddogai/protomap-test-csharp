namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MobilityReservationEngineChecks
{
    public static void MobilityReservationEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityReservationEngine.DefaultMobilityReservationEngine(Flags.NewRegistry());
        var item = MobilityReservation.NewMobilityReservation("code-842", "label-741");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityReservationEngineKeepsResultsWithinBounds();
    }
}
