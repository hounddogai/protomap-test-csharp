namespace Acme.Workforce.Contractors.Tests.Rate.RateReview;

using Acme.Workforce.Contractors.Rate.RateReview;

/// <summary>Checks of rate review code, run by the project's runner.</summary>
public static class GlobalRateReviewEngineChecks
{
    public static void GlobalRateReviewEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalRateReviewEngine.DefaultGlobalRateReviewEngine();
        var item = GlobalRateReview.NewGlobalRateReview("code-216", "label-528");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalRateReviewEngineKeepsResultsWithinBounds();
    }
}
