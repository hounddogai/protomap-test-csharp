namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledPositionTemplateEngineChecks
{
    public static void ScheduledPositionTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPositionTemplateEngine.DefaultScheduledPositionTemplateEngine();
        var item = ScheduledPositionTemplate.NewScheduledPositionTemplate("code-222", "title-66");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPositionTemplateEngineKeepsResultsWithinBounds();
    }
}
