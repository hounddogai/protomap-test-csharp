namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanBudgetEngineChecks
{
    public static void SpanBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = SpanBudgetEngine.DefaultSpanBudgetEngine();
        var item = SpanBudget.NewSpanBudget("code-591", "name-370");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanBudgetEngineKeepsResultsWithinBounds();
    }
}
