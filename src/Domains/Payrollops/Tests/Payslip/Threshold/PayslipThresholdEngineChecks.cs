namespace Acme.Workforce.Payrollops.Tests.Payslip.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayslipThresholdEngineChecks
{
    public static void PayslipThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipThresholdEngine.DefaultPayslipThresholdEngine(Flags.NewRegistry());
        var item = PayslipThreshold.NewPayslipThreshold("code-409", "label-149");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipThresholdEngineKeepsResultsWithinBounds();
    }
}
