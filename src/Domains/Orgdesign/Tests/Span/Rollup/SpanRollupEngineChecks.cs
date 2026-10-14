namespace Acme.Workforce.Orgdesign.Tests.Span.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SpanRollupEngineChecks
{
    public static void SpanRollupEngineKeepsResultsWithinBounds()
    {
        var engine = SpanRollupEngine.DefaultSpanRollupEngine(Flags.NewRegistry());
        var item = SpanRollup.NewSpanRollup("code-807", "name-805");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SpanRollupEngineKeepsResultsWithinBounds();
    }
}
