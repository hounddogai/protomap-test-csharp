namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

/// <summary>Checks of payslip assignment code, run by the project's runner.</summary>
public static class FallbackPayslipCheckpointEngineChecks
{
    public static void FallbackPayslipCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackPayslipCheckpointEngine.DefaultFallbackPayslipCheckpointEngine(Flags.NewRegistry());
        var item = FallbackPayslipCheckpoint.NewFallbackPayslipCheckpoint("code-48", "name-255");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackPayslipCheckpointEngineKeepsResultsWithinBounds();
    }
}
