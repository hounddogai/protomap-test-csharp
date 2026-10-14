namespace Acme.Workforce.Orgdesign.Tests.Headcount.Budget;

using Acme.Workforce.Orgdesign.Headcount.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class BatchedHeadcountTemplateEngineChecks
{
    public static void BatchedHeadcountTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedHeadcountTemplateEngine.DefaultBatchedHeadcountTemplateEngine();
        var item = BatchedHeadcountTemplate.NewBatchedHeadcountTemplate("code-9", "name-529");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedHeadcountTemplateEngineKeepsResultsWithinBounds();
    }
}
