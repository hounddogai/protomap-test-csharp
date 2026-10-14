namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class DefaultExceptionReviewEngineChecks
{
    public static void DefaultExceptionReviewEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultExceptionReviewEngine.DefaultDefaultExceptionReviewEngine();
        var item = DefaultExceptionReview.NewDefaultExceptionReview("code-815", "label-780");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultExceptionReviewEngineKeepsResultsWithinBounds();
    }
}
