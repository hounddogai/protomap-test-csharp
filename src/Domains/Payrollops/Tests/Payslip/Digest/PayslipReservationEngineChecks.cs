namespace Acme.Workforce.Payrollops.Tests.Payslip.Digest;

using Acme.Workforce.Payrollops.Payslip.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class PayslipReservationEngineChecks
{
    public static void PayslipReservationEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipReservationEngine.DefaultPayslipReservationEngine();
        var item = PayslipReservation.NewPayslipReservation("code-666", "name-430");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipReservationEngineKeepsResultsWithinBounds();
    }
}
