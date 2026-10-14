namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DeductionScoreEngineChecks
{
    public static void DeductionScoreEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionScoreEngine.DefaultDeductionScoreEngine(Flags.NewRegistry());
        var item = DeductionScore.NewDeductionScore("code-484", "title-776");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionScoreEngineKeepsResultsWithinBounds();
    }
}
