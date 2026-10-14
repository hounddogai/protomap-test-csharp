namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentScoreEngineChecks
{
    public static void AdjustmentScoreEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentScoreEngine.DefaultAdjustmentScoreEngine(Flags.NewRegistry());
        var item = AdjustmentScore.NewAdjustmentScore("code-284", "title-374");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentScoreEngineKeepsResultsWithinBounds();
    }
}
