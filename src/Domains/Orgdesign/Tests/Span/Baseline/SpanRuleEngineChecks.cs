namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanRuleEngineChecks
{
    public static void SpanRuleEngineKeepsResultsWithinBounds()
    {
        var engine = SpanRuleEngine.DefaultSpanRuleEngine(Flags.NewRegistry());
        var item = SpanRule.NewSpanRule("code-203", "title-899");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanRuleEngineKeepsResultsWithinBounds();
    }
}
