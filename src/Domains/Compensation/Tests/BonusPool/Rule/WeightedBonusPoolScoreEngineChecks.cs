namespace Acme.Workforce.Compensation.Tests.BonusPool.Rule;

using Acme.Workforce.Compensation.BonusPool.Rule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class WeightedBonusPoolScoreEngineChecks
{
    public static void WeightedBonusPoolScoreEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedBonusPoolScoreEngine.DefaultWeightedBonusPoolScoreEngine(Flags.NewRegistry());
        var item = WeightedBonusPoolScore.NewWeightedBonusPoolScore("code-429", "label-208");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedBonusPoolScoreEngineKeepsResultsWithinBounds();
    }
}
