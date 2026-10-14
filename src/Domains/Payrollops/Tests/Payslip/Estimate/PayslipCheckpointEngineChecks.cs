namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipCheckpointEngineChecks
{
    public static void PayslipCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = PayslipCheckpointEngine.DefaultPayslipCheckpointEngine();
        var item = PayslipCheckpoint.NewPayslipCheckpoint("code-134", "name-543");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayslipCheckpointEngineKeepsResultsWithinBounds();
    }
}
