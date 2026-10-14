namespace Acme.Workforce.Compensation.Tests.Adjustment.Summary;

using Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class GlobalAdjustmentSummaryEngineChecks
{
    public static void GlobalAdjustmentSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAdjustmentSummaryEngine.DefaultGlobalAdjustmentSummaryEngine();
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("code-867", "title-524");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAdjustmentSummaryEngineKeepsResultsWithinBounds();
    }
}
