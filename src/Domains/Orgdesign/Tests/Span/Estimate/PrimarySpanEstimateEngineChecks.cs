namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimarySpanEstimateEngineChecks
{
    public static void PrimarySpanEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = PrimarySpanEstimateEngine.DefaultPrimarySpanEstimateEngine();
        var item = PrimarySpanEstimate.NewPrimarySpanEstimate("code-171", "label-964");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimarySpanEstimateEngineKeepsResultsWithinBounds();
    }
}
