namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class ManualSpanPlanEngineChecks
{
    public static void ManualSpanPlanEngineKeepsResultsWithinBounds()
    {
        var engine = ManualSpanPlanEngine.DefaultManualSpanPlanEngine();
        var item = ManualSpanPlan.NewManualSpanPlan("code-511", "name-23");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualSpanPlanEngineKeepsResultsWithinBounds();
    }
}
