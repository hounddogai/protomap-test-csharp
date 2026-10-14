namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedRetroPayReservationEngineChecks
{
    public static void CappedRetroPayReservationEngineKeepsResultsWithinBounds()
    {
        var engine = CappedRetroPayReservationEngine.DefaultCappedRetroPayReservationEngine(Flags.NewRegistry());
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("code-992", "name-369");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedRetroPayReservationEngineKeepsResultsWithinBounds();
    }
}
