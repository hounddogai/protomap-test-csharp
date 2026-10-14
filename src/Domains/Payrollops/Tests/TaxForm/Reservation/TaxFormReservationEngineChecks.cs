namespace Acme.Workforce.Payrollops.Tests.TaxForm.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.TaxForm.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TaxFormReservationEngineChecks
{
    public static void TaxFormReservationEngineKeepsResultsWithinBounds()
    {
        var engine = TaxFormReservationEngine.DefaultTaxFormReservationEngine(Flags.NewRegistry());
        var item = TaxFormReservation.NewTaxFormReservation("code-360", "label-888");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TaxFormReservationEngineKeepsResultsWithinBounds();
    }
}
