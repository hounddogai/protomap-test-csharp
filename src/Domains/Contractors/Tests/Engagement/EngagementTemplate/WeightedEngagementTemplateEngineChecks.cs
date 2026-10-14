namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class WeightedEngagementTemplateEngineChecks
{
    public static void WeightedEngagementTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedEngagementTemplateEngine.DefaultWeightedEngagementTemplateEngine();
        var item = WeightedEngagementTemplate.NewWeightedEngagementTemplate("code-898", "title-370");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedEngagementTemplateEngineKeepsResultsWithinBounds();
    }
}
