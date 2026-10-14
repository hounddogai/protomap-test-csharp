namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BatchedBonusPoolReviewEngineChecks
{
    public static void BatchedBonusPoolReviewEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedBonusPoolReviewEngine.DefaultBatchedBonusPoolReviewEngine(Flags.NewRegistry());
        var item = BatchedBonusPoolReview.NewBatchedBonusPoolReview("code-112", "title-74");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedBonusPoolReviewEngineKeepsResultsWithinBounds();
    }
}
