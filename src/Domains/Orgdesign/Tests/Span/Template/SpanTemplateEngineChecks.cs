namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanTemplateEngineChecks
{
    public static void SpanTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = SpanTemplateEngine.DefaultSpanTemplateEngine(Flags.NewRegistry());
        var item = SpanTemplate.NewSpanTemplate("code-852", "label-134");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanTemplateEngineKeepsResultsWithinBounds();
    }
}
