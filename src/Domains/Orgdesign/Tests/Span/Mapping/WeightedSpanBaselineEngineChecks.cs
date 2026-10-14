namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class WeightedSpanBaselineEngineChecks
{
    public static void WeightedSpanBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedSpanBaselineEngine.DefaultWeightedSpanBaselineEngine(Flags.NewRegistry());
        var item = WeightedSpanBaseline.NewWeightedSpanBaseline("code-958", "title-987");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedSpanBaselineEngineKeepsResultsWithinBounds();
    }
}
