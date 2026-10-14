namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorTemplateEngineChecks
{
    public static void VisitorTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorTemplateEngine.DefaultVisitorTemplateEngine();
        var item = VisitorTemplate.NewVisitorTemplate("code-381", "name-824");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorTemplateEngineKeepsResultsWithinBounds();
    }
}
