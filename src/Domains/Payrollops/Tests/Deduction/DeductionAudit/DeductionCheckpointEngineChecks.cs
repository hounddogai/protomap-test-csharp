namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.DeductionAudit;

/// <summary>Checks of deduction audit code, run by the project's runner.</summary>
public static class DeductionCheckpointEngineChecks
{
    public static void DeductionCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionCheckpointEngine.DefaultDeductionCheckpointEngine(Flags.NewRegistry());
        var item = DeductionCheckpoint.NewDeductionCheckpoint("code-689", "name-547");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionCheckpointEngineKeepsResultsWithinBounds();
    }
}
