namespace Acme.Workforce.Contractors.Tests.Rate.Review;

using Acme.Workforce.Contractors.Rate.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RateReviewEngineChecks
{
    public static void RateReviewEngineKeepsResultsWithinBounds()
    {
        var engine = RateReviewEngine.DefaultRateReviewEngine();
        var item = RateReview.NewRateReview("code-112", "name-982");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateReviewEngineKeepsResultsWithinBounds();
    }
}
