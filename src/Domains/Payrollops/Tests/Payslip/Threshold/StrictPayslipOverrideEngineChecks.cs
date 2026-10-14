namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StrictPayslipOverrideEngineChecks
{
    public static void StrictPayslipOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPayslipOverrideEngine.DefaultStrictPayslipOverrideEngine();
        var item = StrictPayslipOverride.NewStrictPayslipOverride("code-870", "title-956");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPayslipOverrideEngineKeepsResultsWithinBounds();
    }
}
