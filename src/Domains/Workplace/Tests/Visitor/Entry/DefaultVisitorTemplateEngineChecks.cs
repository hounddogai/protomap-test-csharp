namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DefaultVisitorTemplateEngineChecks
{
    public static void DefaultVisitorTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultVisitorTemplateEngine.DefaultDefaultVisitorTemplateEngine(Flags.NewRegistry());
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("code-950", "name-559");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultVisitorTemplateEngineKeepsResultsWithinBounds();
    }
}
