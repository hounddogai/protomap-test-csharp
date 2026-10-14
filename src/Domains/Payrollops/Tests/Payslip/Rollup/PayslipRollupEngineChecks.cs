namespace Acme.Workforce.Payrollops.Tests.Payslip.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayslipRollupEngineChecks
{
    public static void PayslipRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipRollupEngine.DefaultPayslipRollupEngine(Flags.NewRegistry());
        var item = PayslipRollup.NewPayslipRollup("code-501", "name-732");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipRollupEngineKeepsResultsWithinBounds();
    }
}
