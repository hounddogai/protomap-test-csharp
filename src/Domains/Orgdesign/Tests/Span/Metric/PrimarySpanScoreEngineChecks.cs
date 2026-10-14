namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PrimarySpanScoreEngineChecks
{
    public static void PrimarySpanScoreEngineKeepsResultsWithinBounds()
    {
        var engine = PrimarySpanScoreEngine.DefaultPrimarySpanScoreEngine();
        var item = PrimarySpanScore.NewPrimarySpanScore("code-400", "label-923");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimarySpanScoreEngineKeepsResultsWithinBounds();
    }
}
