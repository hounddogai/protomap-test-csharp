namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalPayslipCheckpointEngineChecks
{
    public static void GlobalPayslipCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPayslipCheckpointEngine.DefaultGlobalPayslipCheckpointEngine();
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("code-107", "name-675");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPayslipCheckpointEngineKeepsResultsWithinBounds();
    }
}
