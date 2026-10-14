namespace Acme.Workforce.Compensation.Tests.MeritCycle.Reservation;

using Acme.Workforce.Compensation.MeritCycle.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MeritCycleReservationEngineChecks
{
    public static void MeritCycleReservationEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleReservationEngine.DefaultMeritCycleReservationEngine(Flags.NewRegistry());
        var item = MeritCycleReservation.NewMeritCycleReservation("code-574", "label-41");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleReservationEngineKeepsResultsWithinBounds();
    }
}
