namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolBaselineEngineChecks
{
    public static void BonusPoolBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolBaselineEngine.DefaultBonusPoolBaselineEngine();
        var item = BonusPoolBaseline.NewBonusPoolBaseline("code-57", "title-23");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolBaselineEngineKeepsResultsWithinBounds();
    }
}
