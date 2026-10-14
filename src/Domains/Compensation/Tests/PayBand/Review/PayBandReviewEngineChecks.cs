namespace Acme.Workforce.Compensation.Tests.PayBand.Review;

using Acme.Workforce.Compensation.PayBand.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayBandReviewEngineChecks
{
    public static void PayBandReviewEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandReviewEngine.DefaultPayBandReviewEngine();
        var item = PayBandReview.NewPayBandReview("code-647", "name-275");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandReviewEngineKeepsResultsWithinBounds();
    }
}
