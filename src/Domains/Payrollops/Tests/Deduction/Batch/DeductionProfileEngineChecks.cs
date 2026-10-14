namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DeductionProfileEngineChecks
{
    public static void DeductionProfileEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionProfileEngine.DefaultDeductionProfileEngine();
        var item = DeductionProfile.NewDeductionProfile("code-798", "title-392");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionProfileEngineKeepsResultsWithinBounds();
    }
}
