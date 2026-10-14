namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictDeductionRuleEngineChecks
{
    public static void StrictDeductionRuleEngineKeepsResultsWithinBounds()
    {
        var engine = StrictDeductionRuleEngine.DefaultStrictDeductionRuleEngine();
        var item = StrictDeductionRule.NewStrictDeductionRule("code-674", "label-252");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictDeductionRuleEngineKeepsResultsWithinBounds();
    }
}
