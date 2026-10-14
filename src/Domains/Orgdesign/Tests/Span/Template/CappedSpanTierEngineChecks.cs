namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CappedSpanTierEngineChecks
{
    public static void CappedSpanTierEngineKeepsResultsWithinBounds()
    {
        var engine = CappedSpanTierEngine.DefaultCappedSpanTierEngine();
        var item = CappedSpanTier.NewCappedSpanTier("code-808", "label-969");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedSpanTierEngineKeepsResultsWithinBounds();
    }
}
