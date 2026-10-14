namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBudget;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.DeductionBudget;

/// <summary>Checks of deduction budget code, run by the project's runner.</summary>
public static class RegionalDeductionTemplateEngineChecks
{
    public static void RegionalDeductionTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalDeductionTemplateEngine.DefaultRegionalDeductionTemplateEngine(Flags.NewRegistry());
        var item = RegionalDeductionTemplate.NewRegionalDeductionTemplate("code-423", "name-942");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalDeductionTemplateEngineKeepsResultsWithinBounds();
    }
}
