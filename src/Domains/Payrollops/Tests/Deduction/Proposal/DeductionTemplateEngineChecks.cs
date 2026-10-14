namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionTemplateEngineChecks
{
    public static void DeductionTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionTemplateEngine.DefaultDeductionTemplateEngine();
        var item = DeductionTemplate.NewDeductionTemplate("code-71", "label-787");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionTemplateEngineKeepsResultsWithinBounds();
    }
}
