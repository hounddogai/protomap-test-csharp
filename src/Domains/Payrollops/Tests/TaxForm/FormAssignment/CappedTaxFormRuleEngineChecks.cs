namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormAssignment;

using Acme.Workforce.Payrollops.TaxForm.FormAssignment;

/// <summary>Checks of form assignment code, run by the project's runner.</summary>
public static class CappedTaxFormRuleEngineChecks
{
    public static void CappedTaxFormRuleEngineKeepsResultsWithinBounds()
    {
        var engine = CappedTaxFormRuleEngine.DefaultCappedTaxFormRuleEngine();
        var item = CappedTaxFormRule.NewCappedTaxFormRule("code-573", "name-111");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedTaxFormRuleEngineKeepsResultsWithinBounds();
    }
}
