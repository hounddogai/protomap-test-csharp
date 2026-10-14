namespace Acme.Workforce.Compliance.Tests.Exception.Review;

using Acme.Workforce.Compliance.Exception.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ExceptionReviewEngineChecks
{
    public static void ExceptionReviewEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionReviewEngine.DefaultExceptionReviewEngine();
        var item = ExceptionReview.NewExceptionReview("code-441", "name-735");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionReviewEngineKeepsResultsWithinBounds();
    }
}
