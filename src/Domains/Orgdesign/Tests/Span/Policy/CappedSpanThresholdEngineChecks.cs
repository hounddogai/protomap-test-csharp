namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CappedSpanThresholdEngineChecks
{
    public static void CappedSpanThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = CappedSpanThresholdEngine.DefaultCappedSpanThresholdEngine();
        var item = CappedSpanThreshold.NewCappedSpanThreshold("code-474", "title-349");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedSpanThresholdEngineKeepsResultsWithinBounds();
    }
}
