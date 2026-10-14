namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class AdaptiveBonusPoolBaselineEngineChecks
{
    public static void AdaptiveBonusPoolBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveBonusPoolBaselineEngine.DefaultAdaptiveBonusPoolBaselineEngine();
        var item = AdaptiveBonusPoolBaseline.NewAdaptiveBonusPoolBaseline("code-808", "label-635");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveBonusPoolBaselineEngineKeepsResultsWithinBounds();
    }
}
