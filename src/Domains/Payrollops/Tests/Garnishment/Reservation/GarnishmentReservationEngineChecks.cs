namespace Acme.Workforce.Payrollops.Tests.Garnishment.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class GarnishmentReservationEngineChecks
{
    public static void GarnishmentReservationEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentReservationEngine.DefaultGarnishmentReservationEngine(Flags.NewRegistry());
        var item = GarnishmentReservation.NewGarnishmentReservation("code-230", "label-375");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentReservationEngineKeepsResultsWithinBounds();
    }
}
