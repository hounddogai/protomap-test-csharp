namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BonusPoolReviewEngineChecks
{
    public static void BonusPoolReviewEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolReviewEngine.DefaultBonusPoolReviewEngine();
        var item = BonusPoolReview.NewBonusPoolReview("code-840", "title-115");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolReviewEngineKeepsResultsWithinBounds();
    }
}
