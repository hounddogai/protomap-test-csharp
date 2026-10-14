namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdaptiveAdjustmentNoticeEngineChecks
{
    public static void AdaptiveAdjustmentNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveAdjustmentNoticeEngine.DefaultAdaptiveAdjustmentNoticeEngine();
        var item = AdaptiveAdjustmentNotice.NewAdaptiveAdjustmentNotice("code-509", "name-395");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveAdjustmentNoticeEngineKeepsResultsWithinBounds();
    }
}
