namespace Acme.Workforce.Payrollops.Tests.Deduction.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class StrictDeductionMilestoneEngineChecks
{
    public static void StrictDeductionMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = StrictDeductionMilestoneEngine.DefaultStrictDeductionMilestoneEngine(Flags.NewRegistry());
        var item = StrictDeductionMilestone.NewStrictDeductionMilestone("code-966", "name-193");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictDeductionMilestoneEngineKeepsResultsWithinBounds();
    }
}
