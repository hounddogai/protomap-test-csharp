namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictBonusPoolVarianceEngineChecks
{
    public static void StrictBonusPoolVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = StrictBonusPoolVarianceEngine.DefaultStrictBonusPoolVarianceEngine();
        var item = StrictBonusPoolVariance.NewStrictBonusPoolVariance("code-411", "label-108");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictBonusPoolVarianceEngineKeepsResultsWithinBounds();
    }
}
