namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DeductionMilestoneEngineChecks
{
    public static void DeductionMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionMilestoneEngine.DefaultDeductionMilestoneEngine();
        var item = DeductionMilestone.NewDeductionMilestone("code-928", "name-634");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionMilestoneEngineKeepsResultsWithinBounds();
    }
}
