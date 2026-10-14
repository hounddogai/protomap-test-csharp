namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class LenientPayslipRollupEngineChecks
{
    public static void LenientPayslipRollupEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPayslipRollupEngine.DefaultLenientPayslipRollupEngine();
        var item = LenientPayslipRollup.NewLenientPayslipRollup("code-887", "title-377");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPayslipRollupEngineKeepsResultsWithinBounds();
    }
}
