namespace Acme.Workforce.Payrollops.Tests.Deduction.Budget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeductionReservationEngineChecks
{
    public static void DeductionReservationEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionReservationEngine.DefaultDeductionReservationEngine(Flags.NewRegistry());
        var item = DeductionReservation.NewDeductionReservation("code-676", "name-63");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionReservationEngineKeepsResultsWithinBounds();
    }
}
