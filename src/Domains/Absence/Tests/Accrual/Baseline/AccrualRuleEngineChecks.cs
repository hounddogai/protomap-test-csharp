namespace Acme.Workforce.Absence.Tests.Accrual.Baseline;

using Acme.Workforce.Absence.Accrual.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AccrualRuleEngineChecks
{
    public static void AccrualRuleEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualRuleEngine.DefaultAccrualRuleEngine(Flags.NewRegistry());
        var item = AccrualRule.NewAccrualRule("code-311", "name-753");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualRuleEngineKeepsResultsWithinBounds();
    }
}
