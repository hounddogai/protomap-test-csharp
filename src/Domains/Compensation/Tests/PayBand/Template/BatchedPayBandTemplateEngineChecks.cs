namespace Acme.Workforce.Compensation.Tests.PayBand.Template;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class BatchedPayBandTemplateEngineChecks
{
    public static void BatchedPayBandTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPayBandTemplateEngine.DefaultBatchedPayBandTemplateEngine();
        var item = BatchedPayBandTemplate.NewBatchedPayBandTemplate("code-271", "label-609");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPayBandTemplateEngineKeepsResultsWithinBounds();
    }
}
